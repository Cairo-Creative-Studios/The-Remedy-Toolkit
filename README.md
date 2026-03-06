# The Remedy Toolkit
*Build your game, not your infrastructure*

*A modular gameplay architecture framework for Unity that automates core infrastructure with minimal runtime overhead and can be integrated into any Unity project.*


[![Unity Version](https://img.shields.io/badge/Unity-2022%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![Unity Version](https://img.shields.io/badge/Unity-6000%2B-blue.svg)](https://unity3d.com/get-unity/download)

## 🎯 What is The Remedy?

* Production-ready gameplay modules with reusable system implementations and extensible architecture    
  → Pooling  
  → Input  
  → Networking  
  → Character controllers  
  → Weapons
  
* Editor-first workflow with instant feedback   
  → See Tooling Overview

* Automated dependency injection with zero manual wiring  
  → See Automation Breakdown
  
* Native-performance asset-driven gameplay graphs    
  → See Graph Runtime & Performance
  
* Zero-allocation signal/event architecture   
  → See Signal Architecture & Benchmarks

* Reflection-free hot paths    
  → See Runtime Architecture
  
## Architecture Philosophy  
The Remedy centralizes common gameplay infrastructure patterns into a unified architecture layer while preserving project flexibility and modular adoption.

### Why The Remedy?
• Reduces boilerplate infrastructure setup  
• Avoids heavy global frameworks  
• Focuses on performance-oriented gameplay systems  

## Adoption Model  
The Remedy can be adopted incrementally:

• Use signals independently  
• Integrate graphs without replacing your systems  
• Enable infrastructure modules as needed  

No forced global architecture.

---

## 🎮 Powered by Remedy Toolkit

Here are some projects and demos built using The Remedy Toolkit:

| Game / Demo | Clip |
|------------|------|
| *Tails Adventure Armada Fangame* | ![Clip](https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExY2hucWU5eGZ6dGF0aWgyMWlvcGtkMWd4ZXN6cDBsNW1kdHpuOGp6eCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/nq8mTu1N24CCXH2Z5z/giphy.gif) |

> All clips are captured in real-time to showcase **performance, node-based workflows, and gameplay features** powered by Remedy Toolkit.

---

## 🚀 Core Features

### 🎨 **Schematics Editor**
![Clip](https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExcjFuOXNxZHQ3dGUybHFxOTlqbHA2aTU1eXAzcjNzeWc3eHg4ZHcydiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/GJL1EekvjLFr5P7mgl/giphy.gif)
- **Visual node-based development** Unreal Blueprint-inspired interface
- **IODock system** for drag-and-drop component wiring backed by automated ScriptableEvent creation and management
- **Prefab-driven architecture** with global system scope for systems like Input and Audio
- **Auto-generated ScriptableObjects** - Zero manual asset management, meaning you simply add a component and it's Input and Output and Data are all neatly generated behind the scenes

### ⚡ **Performance-First Engine**
![Clip](https://github.com/Cairo-Creative-Studios/The-Remedy-Toolkit/blob/main/media/character%20controller%20performance.gif)
- **Union-based value passing** - CPU cache optimized data structures with 0 GC Alloc for almost all Unity relevant Types
- **Zero reflection at runtime** - Pure ScriptableObject architecture
- **Automatic object pooling** for Schematic-based objects
- **Sub-0.1ms frame time** for complex physics based character controllers
- **Multiplatform** runs blazing fast on all platforms

### 🔧 **Gameplay Features**
![Clip](https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExOHloaXFkb3duaWM0azA3MzZvY2dqZTl2NGRxNXBra3hrbnFxbzhjMSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/SAP1lRPy721vcJdOVD/giphy.gif)
- **Character Controllers**: Hover, wall-slide, ledge-grab systems
- **Weapon System**: Projectile and melee weapons for any object type
- **Advanced Camera**: Flexible Cameras including a AAA-quality third-person camera (Mario Odyssey-style) and over the shoulder shooter camera 
- **Custom UI System**: Mesh-based UI with better performance than built in Unity UI frameworks
- **Audio Management**: Full control over the audio of the game from music to sound, sound cues are pooled elegantly 
- **Inventory Management**: Manage inventories, item collection, and more, use in conjunction with previous features for weapon pickup or displaying in the UI
- **Damageables**: Health and Damage management for characters and objects 

### 🏗️ **Enterprise Architecture**
- **Complete decoupling** - Remove any feature without errors
- **SOLID compliance** enforced by design
- **Event-driven communication** via automated ScriptableEvents  
- **Modular by default** - Pay only for what you use

---

## 📊 Performance Comparison

| System | Frame Time | Performance Factor |
|--------|------------|-------------------|
| Unity Visual Scripting | ~1.5ms | 1x (baseline) |
| **Remedy Toolkit** | **~0.1ms** | **15x faster** |

*Tested with full-featured rigidbody character controller including sphere-cast ground and wall detection*

---

### Your First Schematic in 60 Seconds
1. **Create a Prefab** The system will ask if you want to create a Schematic, click Yes
2. **Open Schematics Window** Opens automatically on first creation, then you can click the $ button where the prefab is displayed in the Object Hierarchy
3. **Setup your Prefab** Set up prefab components faster than the Unity Inspector using the Schematic IODock 
4. **Connect nodes** Drag-and-drop Events from the IODock to the Schematic Graph and wire them up to make your game object come to life
5. **Hit Play** - Watch it come to life

### Implement Schematics in Your Own Scripts:
```csharp
public class PlayerMovement : MonoBehaviour
{
    // Your component just needs to use Scriptable Events and it the Schematic Editor will pick find it and set it up for you...
    public ScriptableEvent<Vector3>.Input OnMoveInput;
    public ScriptableEvent<bool>.Output OnGrounded;

    // The Schematic Editor will also generate data for properties if you use the SchematicProperties Attribute...
    [SchematicProperties]
    public MyDataSO Properties;

    // Enable User Created Event Setups using IdentityLists... IdentityLists can use Identifier Fields to tell the items apart, which are fields that are customizable by the user using the given Identifier Type
    [IdentityListRenderer(identifierType: EventListIdentifierType.Name, identifierField: "Name", depth: 0, foldoutTitle: "Custom Events", itemName: "Event")]
    public List<CustomizableEvent> CustomEvents = new(); // You can also use arrays: CustomizableEvent[]

    private bool _isOnGround = false;

    private void OnEnable()
    {
      OnMoveInput.Subscribe(this, //function or lambda);
    }

    private void OnDisable()
    {
      OnMoveInput.Unsubscribe(this);
    }

    //...

    private void Update()
    {
      // Ground Check
      OnGrounded?.Invoke(_isOnGround);
    }

    [Serializable]
    public class CustomizableEvent 
    {
        public string Name;

        // All fields/properties of items in the list are also managed by the IODock
        [IdentityListRenderer(identifierType: EventListIdentifierType.Dropdown, identifierField: "Type", depth: 1, foldoutTitle: "Nested Events", itemName: "Event")]
        public NestedEvent[] NestedEvents = new NestedEvents[0];
        public ScriptableEvent.Output CustomOutput;
        [SchematicProperties]
        public CustomProperties Properties;

        [Serializable]
        public class NestedEvent
        {
          //...
        }
    }
}
```


---

## 🏢 Professional Services

### Custom Development
Looking for a **Unity architecture specialist** who can:
- **Rescue legacy projects** from performance hell
- **Build scalable systems** that grow with your team  
- **Implement SOLID principles** in Unity environments
- **Create custom tooling** that matches (or at least does not conflict with) your workflow

### Consulting & Training  
- **Code reviews** and architecture audits
- **Team training** on advanced Unity patterns
- **Performance optimization** consulting
- **Custom framework development**

**Contact**: [TheChayed@Gmail.com]

---

## 📈 Why Choose The Remedy?

### For Solo Developers
- **Rapid prototyping** without technical debt
- **Professional architecture** from day one  
- **No performance compromises** as you scale

### For Teams  
- **Designer-friendly** visual interface
- **Programmer-approved** performance and architecture
- **Reduced onboarding time** with intuitive workflows

### For Enterprises
- **Maintainable codebases** that survive team changes
- **Modular architecture** supporting multiple projects
- **Performance guarantees** for shipped products

---

## 🔮 Roadmap

- [ ] **Schematic Singletons** - Global Schematic 
- [ ] **Timeline Control** - Interact with the Timeline with Nodes and call ScriptableEvents or vice versa
- [ ] **Enemy AI** - Communicate with the new Behavior tree package that Unity recently released for Enemy Behavior 
- [ ] **Level Streaming** - Editor tooling to allow designers to create level chunks in scene and stream them into play
- [ ] **Multiplayer Integration** - NetworkEvents for ScriptableEvents so they can optionally be synced via multiplayer, and also incorporate multiplayer in feature components
- [ ] **Analytics Integration** - Analytics functionality to track player behavior using the Schematic Graph
- [ ] **Asset Store Release** - Public availability
- [ ] **Documentation Portal** - Comprehensive guides and tutorials

---

## 📝 Documentation

- [**Getting Started Guide**](docs/getting-started.md)
- [**Architecture Overview**](docs/architecture.md) 
- [**Performance Deep Dive**](docs/performance.md)
- [**API Reference**](docs/api-reference.md)

---

## 🤝 Contributing

This toolkit represents years of solving Unity's core architectural problems. If you're interested in contributing to the future of Unity development, please:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)  
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

All contributors whose Pull Requests are accepted will **receive a free commercial license key** for the Toolkit upon release, as thanks for helping shape its future.

---

## 📄 License

This project is **proprietary software**. All rights are reserved by Cairo Creative Studios, as designated by the creator of the Toolkit, Chad Wolfe.  

- The source code is provided for evaluation and contribution purposes only.  
- You may not copy, modify, or redistribute this project outside of submitting contributions through Pull Requests.  
- Commercial use requires a license. Inquiries can be made at **[TheChayed@Gmail.com]**.  
- Contributors with accepted Pull Requests will be granted a free commercial license key.  
- A commercial license key grants the right to **use the Remedy Toolkit in commercial projects**, but **does not grant redistribution or resale rights**.  

---

## ⭐ Support the Project

If The Remedy Toolkit has saved you development time and improved your Unity workflow, please:
- **Star this repository** ⭐
- **Share with your Unity community** 🔄  
- **Follow for updates** 👁️
- **Consider hiring me** for your next Unity project 💼

---

*"Don't just build games. Build them right."* - The Remedy Toolkit
