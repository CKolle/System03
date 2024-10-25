using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System03.FileFormats;
using System03.Rendering.Graphics;

namespace System03.Rendering.OpenGL;

public class Renderer : IRenderer
{

    private IGraphicsContext _context;
    private const string VertexShaderSource = @"#version 300 es
        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec2 aTexCoord;
        layout(location = 2) in vec3 aNormal;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

        out vec2 vTexCoord;
        out vec3 vNormal;
        out vec3 vFragPos;

        void main()
        {
            gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
            vTexCoord = aTexCoord;
            vNormal = mat3(transpose(inverse(uModel))) * aNormal;
            vFragPos = vec3(uModel * vec4(aPosition, 1.0));
        }";

    private const string FragmentShaderSource = @"#version 300 es
        precision mediump float;

    in vec2 vTexCoord;
        in vec3 vNormal;
        in vec3 vFragPos;

    uniform sampler2D uTexture;
    uniform vec3 uLightPos;
    uniform vec3 uViewPos;
    uniform bool uHasTexture;
    uniform vec3 uMaterialColor;

    out vec4 FragColor;

    void main()
    {
        vec3 ambient = 0.3 * (uHasTexture ? texture(uTexture, vTexCoord).rgb : uMaterialColor);
    
        vec3 norm = normalize(vNormal);
        vec3 lightDir = normalize(uLightPos - vFragPos);
    
        float diff = max(dot(norm, lightDir), 0.0);
        diff = diff * 0.7 + 0.3; // This prevents completely dark areas, we should probably remove this but needed 
        // needed it to see the model properly in testing
        vec3 diffuse = diff * (uHasTexture ? texture(uTexture, vTexCoord).rgb : uMaterialColor);
    
        vec3 viewDir = normalize(uViewPos - vFragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16.0); // Reduced from 32.0
        vec3 specular = 0.3 * spec * vec3(1.0);

        FragColor = vec4(ambient + diffuse + specular, 1.0);
    }";

    private Shader _shader;
    private Matrix4 _viewMatrix;
    private Matrix4 _projectionMatrix;
    private Model _model;

    public Renderer(IGraphicsContext context)
    {
        _context = context;
    }
    public void Initialize()
    {
        // Note we initialize the graphics context here.
        // Because it is the renderer that owns the graphics context
        _context.Initialize();
        _context.MakeCurrent();
        GL.LoadBindings(_context);
        var loadBindingsError = GL.GetError();
        if (loadBindingsError != ErrorCode.NoError)
        {
            throw new Exception($"Error loading OpenGL bindings: {loadBindingsError}");
        }
        
        // Notes for self. If anyone else reads this and it is wrong, please correct me
        // Makes so that the back of the faces are not rendered
        GL.Enable(EnableCap.CullFace);
        // Tells OpenGL to discard the back faces or cull as it is also called apperently
        GL.CullFace(CullFaceMode.Back);
        // Tells OpenGL to render the front faces in counter clockwise order
        // Default open gl is counter clockwise, but I said it implicitly because I got issues on a different computer
        GL.FrontFace(FrontFaceDirection.Ccw);
        // Ensures that objects close to the camera will occlude objects further away
        GL.Enable(EnableCap.DepthTest);
        // Tells OpenGL to  accept a fragment if its depth is less than the current value.
        // Simply put, it makes sure that the fragment is closer to the camera than the current fragment
        GL.DepthFunc(DepthFunction.Less);

        // TODO: Remove this test code
        string glVersion = GL.GetString(StringName.Version);
        Console.WriteLine($"OpenGL version: {glVersion}");

        _shader = new Shader(VertexShaderSource, FragmentShaderSource);
        
        var modelFolderPath = "/home/christopher/Dokumenter/uis/dat240/System03/System03.Launcher/Assets";
        var mikuObj = new ObjFile($"{modelFolderPath}/HatsuneMiku.obj");
        Dictionary<string, Material> materials = LoadMaterials(mikuObj, modelFolderPath);
        
        var mesh = new OpenGLMesh(mikuObj);
        var submeshes = CreateSubmeshes(mikuObj, materials);
        
        // Create the model
        
        _model = new Model(mesh, submeshes)
        {
            Position = new Vector3(0, -7, -40),
            Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(0)), 
            Scale = new Vector3(0.1f, 0.1f, 0.1f),
            
        };

        _viewMatrix = Matrix4.LookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.UnitY);
        _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            800f / 600f,
            0.1f,
            1000f
        );
        
        // TODO: End removal here
    }

    private static Dictionary<string, Material> LoadMaterials(ObjFile objFile, string modelFolderPath)
    {
        var materials = new Dictionary<string, Material>
        {
            // Add a default material in case the model doesn't have any materials
            // So we can still see the model
            { "default", new Material
        {
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            SpecularColor = new Vector3(1.0f, 1.0f, 1.0f),
            Shininess = 32.0f
        } } };

        if (!string.IsNullOrEmpty(objFile.MaterialLibrary))
        {
            var materialFile = new MtlFile($"{modelFolderPath}/{objFile.MaterialLibrary}");
            foreach (var mtlMaterial in materialFile.Materials)
            {
                ITexture? diffuseTexture = null;
                if (!string.IsNullOrEmpty(mtlMaterial.DiffuseTextureMap))
                {
                    diffuseTexture = new OpenGLTexture($"{modelFolderPath}/{mtlMaterial.DiffuseTextureMap}");
                }

                materials.Add(mtlMaterial.Name, Material.FromMtlMaterial(mtlMaterial, diffuseTexture));
            }
        }
        return materials;
    }
    private static List<Submesh> CreateSubmeshes(ObjFile objFile, Dictionary<string, Material> materials)
    {
        var submeshes = new List<Submesh>();

        // Process each material group
        foreach (var (materialName, faces) in objFile.MaterialGroups)
        {
            if (!materials.TryGetValue(materialName, out var material))
            {
                Console.WriteLine($"Material not found: {materialName}, using default");
                material = materials["default"];
            }

            int indexCount = faces.Sum(face => face.VertexIndices.Length);
            
            var submesh = new Submesh
            {
                // Calculate start index based on previous submeshes
                StartIndex = submeshes.Sum(s => s.IndexCount),
                IndexCount = indexCount,
                Material = material
            };

            submeshes.Add(submesh);
        }

        // Handle faces without material if any
        var unmaterializedFaces = objFile.Faces.Where(f => string.IsNullOrEmpty(f.MaterialName)).ToList();
        if (unmaterializedFaces.Any())
        {
            var defaultSubmesh = new Submesh
            {
                StartIndex = submeshes.Sum(s => s.IndexCount),
                IndexCount = unmaterializedFaces.Sum(face => face.VertexIndices.Length),
                Material = materials["default"]
            };
            submeshes.Add(defaultSubmesh);
        }

        return submeshes;
    }


    // TODO: Add support for "vp" lines in the OBJ. Need to investigate if we really need this
    public void RenderModel(Model model, Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {

        _shader.Use();
        
        // Set shared uniforms
        _shader.SetMatrix4("uModel", model.GetModelMatrix());
        _shader.SetMatrix4("uView", viewMatrix);
        _shader.SetMatrix4("uProjection", projectionMatrix);
        
        // Adjust light position to be more suitable for the model's position
        Vector3 lightPos = new Vector3(0.0f, 50.0f, -25.0f);  // Positioned above and slightly in front
        _shader.SetVector3("uLightPos", lightPos);
        _shader.SetVector3("uViewPos", new Vector3(0.0f, 0.0f, 5.0f));

        model.Mesh.Bind();

        foreach (var submesh in model.Submeshes)
        {
            var material = submesh.Material;

            // Handle texture
            if (material.DiffuseTexture != null)
            {
                material.DiffuseTexture.Bind(0);
                _shader.SetInt("uHasTexture", 1);
                _shader.SetInt("uTexture", 0);
            }
            else
            {
                _shader.SetInt("uHasTexture", 0);
                // Use the material's diffuse color instead of hardcoded orange
                _shader.SetVector3("uMaterialColor", material.DiffuseColor);
            }

            // Set material properties
            _shader.SetVector3("uDiffuseColor", material.DiffuseColor);
            _shader.SetVector3("uSpecularColor", material.SpecularColor);
            _shader.SetFloat("uShininess", material.Shininess);

            GL.DrawElements(
                PrimitiveType.Triangles, 
                submesh.IndexCount, 
                DrawElementsType.UnsignedInt, 
                (IntPtr)(submesh.StartIndex * sizeof(uint))
            );
            
            if (material.DiffuseTexture != null)
            {
                material.DiffuseTexture.Unbind();
            }
        }

        model.Mesh.Unbind();
    }
    
    public void Render()
    {
        
        // TODO - remove this test code
        /*GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);*/
        //RenderModel(_model, _viewMatrix, _projectionMatrix);
        //_testRotation += 1.0f;
        //_model.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(_testRotation));
        // TODO - end removal here
        // We swap the buffer to see what we have rendered
        _context.SwapBuffers();
        
        // Clear the screen for the next render
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }
}
