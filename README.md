# The Remedy Toolkit
*Taming Unity's Chaos with Performance-First Architecture*

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![Performance](https://img.shields.io/badge/Performance-15x%20Faster-brightgreen.svg)](#performance)

## 🎯 What is The Remedy?

The Remedy Toolkit solves Unity's biggest development pain points: **performance bottlenecks, tight coupling, and workflow complexity**. Built from the ground up with SOLID principles and performance optimization, it delivers the reliability and ease-of-use that Unity developers have been missing.

### The Problem
- **Unity Visual Scripting**: 15x slower performance overhead
- **Existing Frameworks**: Tight coupling makes customization a nightmare  
- **Workflow Chaos**: Many tools and frameworks as well as ScriptableObject archetectures require jumping between inspectors and managing hundreds of references
- **Scale Issues**: While using traditional ScriptableEvent workflows the mental complexity between event connections grows exponentially for sending and recieving (20² = 400 mental variables to track)

### The Solution, The Remedy Toolkit + Schematics
✅ **15x Performance Improvement** over Unity's Visual Scripting due to proper caching, and zero reflection or boxing

✅ **Zero Coupling** - Plug and play any component without breaking existing systems, all gameplay features can be remixed or completely removed without affecting any other system

✅ **One-Window Workflow** - Build entire gameplay systems without context switching  

✅ **SOLID Architecture** - Enforced design principles that scale beautifully  

---

## 🚀 Core Features

### 🎨 **Schematics Editor**
- **Visual node-based development** Unreal Blueprint-inspired interface
- **IODock system** for drag-and-drop component wiring backed by automated ScriptableEvent creation and management
- **Prefab-driven architecture** with global system scope for systems like Input and Audio
- **Auto-generated ScriptableObjects** - Zero manual asset management, meaning you simply add a component and it's Input and Output and Data are all neatly generated behind the scenes

### ⚡ **Performance-First Engine**
- **Union-based value passing** - CPU cache optimized data structures with 0 GC Alloc for almost all Unity relevant Types
- **Zero reflection at runtime** - Pure ScriptableObject architecture
- **Automatic object pooling** for Schematic-based objects
- **Sub-0.1ms frame time** for complex physics based character controllers
- **Multiplatform** runs blazing fast on all platforms

### 🔧 **Gameplay Features**
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
3. **Setup your Prefab** Use the Schematic IODock to set up your prefab's Children and Components in less time than with the standard unity Inspector 
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

## 📈 Why Choose Remedy?

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

- [ ] **Multiplayer Integration** - Add NetworkEvents to ScriptableEvents so they can optionally be synced via multiplayer, and also incorporate multiplayer in feature components
- [ ] **Asset Store Release** - Public availability
- [ ] **Documentation Portal** - Comprehensive guides and tutorials

---

## 📝 Documentation

- [**Getting Started Guide**](docs/getting-started.md)
- [**Architecture Overview**](docs/architecture.md) 
- [**Performance Deep Dive**](docs/performance.md)
- [**API Reference**](docs/api-reference.md)
- [**Migration Guide**](docs/migration.md)

---

## 🤝 Contributing

This toolkit represents years of solving Unity's core architectural problems. If you're interested in contributing to the future of Unity development, please:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)  
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

All contributors with Pull Requests that are accepted will recieve a free key use the Toolkit commercially.

---

## 📄 License

This project has no license, but all rights for the work go to Cairo Creative Studios as designated by the creator of the Toolkit, Chad Wolfe

---

## ⭐ Support the Project

If The Remedy Toolkit has saved you development time and improved your Unity workflow, please:
- **Star this repository** ⭐
- **Share with your Unity community** 🔄  
- **Follow for updates** 👁️
- **Consider hiring me** for your next Unity project 💼

---

*"Don't just build games. Build them right."* - The Remedy Toolkit
