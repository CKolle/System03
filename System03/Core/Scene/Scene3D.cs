using OpenTK.Mathematics;
using System03.Core.ECS;
using System03.Core.ECS.Component;
using System03.Core.ECS.System;
using System03.FileFormats;
using System03.Input;
using System03.Rendering;
using System03.Rendering.OpenGL;

namespace System03.Core.Scene;

struct ModelComponent : IComponent
{
    // TODO - This is suboptimal, but I don
    public Model Model;
}

/// <summary>
/// If we have multiple cameras we say that the first camera is the active camera
/// </summary>
public struct CameraComponent : IComponent
{
    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;
    public float FieldOfView;
    public float AspectRatio;
    public float NearPlane;
    public float FarPlane;
    
    public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Target, Up);
    public Matrix4 GetProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(
        MathHelper.DegreesToRadians(FieldOfView), 
        AspectRatio, 
        NearPlane, 
        FarPlane);
}

public class CameraSystem(IInputSystem inputSystem) : ISystem
{
    private IKeyboardInputDevice? Keyboard => inputSystem.GetDevice<IKeyboardInputDevice>();
    private float _moveSpeed = 10f;
    public void Update(ComponentManager componentManager, double deltaTime)
    {
        
        if (Keyboard == null) return;
        var entities = componentManager.GetEntitiesWithComponent<CameraComponent>();
        foreach (var entity in entities)
        {
            ref var camera = ref componentManager.GetComponent<CameraComponent>(entity);
            var isWPressed = Keyboard.IsKeyDown(Key.W);
            var isSPressed = Keyboard.IsKeyDown(Key.S);
            var isAPressed = Keyboard.IsKeyDown(Key.A);
            var isDPressed = Keyboard.IsKeyDown(Key.D);
            
            var forward = camera.Target - camera.Position;
            forward.Normalize();
            var right = Vector3.Cross(forward, camera.Up);
            right.Normalize();
            
            var moveSpeed =_moveSpeed * (float)deltaTime;
            
            if (isWPressed)
            {
                camera.Position += forward * moveSpeed;
                camera.Target += forward * moveSpeed;
            }
            
            if (isSPressed)
            {
                camera.Position -= forward * moveSpeed;
                camera.Target -= forward * moveSpeed;
            }
            
            if (isAPressed)
            {
                camera.Position -= right * moveSpeed;
                camera.Target -= right * moveSpeed;
            }
            
            if (isDPressed)
            {
                camera.Position += right * moveSpeed;
                camera.Target += right * moveSpeed;
            }
            
            
        }
    }
}

// TODO I am putting some Miku test code here remove this later. This should be a transform component
class MikuRotateSystem : ISystem
{
    private readonly float _rotationSpeed = 5f;
    
    public void Update(ComponentManager componentManager, double deltaTime)
    {
        componentManager.ForEachComponent((Entity _, ref ModelComponent component) =>
        {
            var newRotation = Quaternion.FromAxisAngle(Vector3.UnitY, _rotationSpeed * (float)deltaTime);
            component.Model.Rotation = newRotation * component.Model.Rotation;
        });
    }
}
class ModelRenderSystem : IRenderSystem
{
    public void Render(ComponentManager componentManager, IRenderer renderer)
    {
        var cameraEntities = componentManager.GetEntitiesWithComponent<CameraComponent>();
        Entity? activeCameraEntity = null;
        foreach (var entity in cameraEntities)
        {
            activeCameraEntity = entity;
        }
        if (activeCameraEntity == null) return;
        // TODO - Find a better way to set the active camera
        var cameraEntity = activeCameraEntity.Value;
        ref var camera = ref componentManager.GetComponent<CameraComponent>(cameraEntity);
        
        var viewMatrix = camera.GetViewMatrix();
        var projectionMatrix = camera.GetProjectionMatrix;
        
        // Render all the models
        componentManager.ForEachComponent((Entity _, ref ModelComponent component) =>
        {
            renderer.RenderModel(component.Model, viewMatrix, projectionMatrix);
        });
    }
}

// Just swaps the buffers for now. TODO - Find a better way to do this maybe in the main game loop
class DoneRenderSystem : IRenderSystem
{
    public void Render(ComponentManager componentManager, IRenderer renderer)
    {
        renderer.Render();
    }
}

// Scene setup extension methods
public static class SceneExtensions
{
    public static Entity CreateCamera(this Scene3D scene, Vector3 position, Vector3 target)
    {
        var entity = scene.CreateEntity();
        scene.ComponentManager.AddComponent(entity, new CameraComponent
        {
            Position = position,
            Target = target,
            Up = Vector3.UnitY,
            FieldOfView = 45f,
            AspectRatio = 1920f / 1080f, // TODO: Get from window/viewport
            NearPlane = 0.1f,
            FarPlane = 1000f
        });
        return entity;
    }
    
    public static Entity CreateModel(this Scene3D scene, Model model, Vector3 position)
    {
        var entity = scene.CreateEntity();
        scene.ComponentManager.AddComponent(entity, new ModelComponent { Model = model });
        return entity;
    }
}


// TODO Find a better way to pass the input system
public class Scene3D(IInputSystem inputSystem) : BaseScene
{
    private IInputSystem _inputSystem = inputSystem;
     private Model LoadMikuModel()
    {
        // TODO - We need to make a better system for laoding in models
        var modelFolderPath = "/home/christopher/Dokumenter/uis/dat240/System03/System03.Launcher/Assets";
        var mikuObj = new ObjFile($"{modelFolderPath}/output.obj");
        
        // Load materials
        var materials = new Dictionary<string, Material>
        {
            { "default", new Material
                {
                    DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
                    SpecularColor = new Vector3(1.0f, 1.0f, 1.0f),
                    Shininess = 32.0f
                }
            }
        };

        if (!string.IsNullOrEmpty(mikuObj.MaterialLibrary))
        {
            var materialFile = new MtlFile($"{modelFolderPath}/{mikuObj.MaterialLibrary}");
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

        // Create mesh and submeshes
        var mesh = new OpenGLMesh(mikuObj);
        var submeshes = new List<Submesh>();

        // Process material groups
        foreach (var (materialName, faces) in mikuObj.MaterialGroups)
        {
            if (!materials.TryGetValue(materialName, out var material))
            {
                Console.WriteLine($"Material not found: {materialName}, using default");
                material = materials["default"];
            }

            int indexCount = faces.Sum(face => face.VertexIndices.Length);
            submeshes.Add(new Submesh
            {
                StartIndex = submeshes.Sum(s => s.IndexCount),
                IndexCount = indexCount,
                Material = material
            });
        }

        // Handle faces without material
        var unmaterializedFaces = mikuObj.Faces.Where(f => string.IsNullOrEmpty(f.MaterialName)).ToList();
        if (unmaterializedFaces.Any())
        {
            submeshes.Add(new Submesh
            {
                StartIndex = submeshes.Sum(s => s.IndexCount),
                IndexCount = unmaterializedFaces.Sum(face => face.VertexIndices.Length),
                Material = materials["default"]
            });
        }

        // Create and return the model
        return new Model(mesh, submeshes)
        {
            Position = new Vector3(0, -2, -40),
            Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(0)),
            Scale = new Vector3(10f, 10f, 10f)
        };
    }
    public override void OnActivate()
    {
        Console.WriteLine("Scene3D Activated - Remove me!");
        base.OnActivate();
        // Lets add some systems
        var mikuEntity = CreateEntity();
        var mikuModel = LoadMikuModel();

        var miku2 = CreateEntity();
        var mikuModel2 = LoadMikuModel();
        mikuModel2.Position = mikuModel2.Position with { X = mikuModel2.Position.X + 6 };
        ComponentManager.AddComponent(mikuEntity, new ModelComponent { Model = mikuModel });
        ComponentManager.AddComponent(miku2, new ModelComponent { Model = mikuModel2 });

        
        this.CreateCamera(new Vector3(0, 0, 5), Vector3.Zero);

        
        AddPreRenderSystem(new CameraSystem(_inputSystem));
        AddPreRenderSystem(new MikuRotateSystem());
        // Lets add a model render system
        AddRenderSystem(new ModelRenderSystem()); 
        
        AddRenderSystem(new DoneRenderSystem());
    }
}