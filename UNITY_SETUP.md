# Unity Setup Guide - Hungry Shark Evolution

## Project Configuration

### 1. Unity Version
- **Recommended**: Unity 2022.3 LTS or newer
- **Minimum**: Unity 2021.3 LTS

### 2. Platform Setup

#### Android Build Settings
```
Platform: Android
Target API Level: API Level 31 (Android 12.0)
Minimum API Level: API Level 24 (Android 7.0)
Scripting Backend: IL2CPP
Target Architectures: ARM64
```

#### iOS Build Settings
```
Platform: iOS
Target iOS Version: 12.0
Target Device: iPhone + iPad
Scripting Backend: IL2CPP
```

### 3. Project Structure Setup

1. **Create Main Scene**:
   - Open `Assets/Scenes/MainGame.unity`
   - Add GameManager prefab to scene
   - Configure camera for 3D underwater view

2. **Setup Shark GameObject**:
   ```
   Shark (GameObject)
   ├── Mesh Renderer (with shark model)
   ├── Rigidbody (useGravity: false, drag: 1)
   ├── Collider (Trigger: true for feeding)
   ├── SharkController.cs
   └── FeedingSystem.cs
   ```

3. **Setup Prey GameObjects**:
   ```
   Fish (Prefab)
   ├── Mesh Renderer (with fish model)
   ├── Rigidbody (useGravity: false, drag: 2)
   ├── Collider
   └── PreyController.cs
   ```

### 4. Component Dependencies

#### Required Components
- **TextMeshPro**: Import via Window > TextMeshPro > Import TMP Essential Resources
- **Input System**: New Input System (optional, currently using legacy)
- **Particle System**: For feeding effects

#### Tags Setup
- Create "Player" tag for Shark
- Create "Prey" tag for fish/food items
- Create "Obstacle" tag for environmental hazards

### 5. Mobile Optimization

#### Graphics Settings
- Use Universal Render Pipeline (URP) for better mobile performance
- Set Texture Quality to Medium for mobile builds
- Enable GPU Instancing for fish spawning

#### Performance Settings
- Target 60 FPS on high-end devices, 30 FPS on low-end
- Use LOD system for distant objects
- Implement object pooling for prey spawning

### 6. Testing Setup

#### Desktop Testing
- Use mouse input to simulate touch
- Test with 16:9 and 18:9 aspect ratios
- Verify all UI elements scale properly

#### Mobile Testing
- Test on multiple screen sizes
- Verify touch sensitivity and responsiveness
- Test gyroscope functionality if available

### 7. Build Configuration

#### Quality Settings
```
Mobile Low: Simple lighting, no shadows
Mobile Medium: Basic shadows, medium textures
Mobile High: Full lighting, high-quality textures
```

#### Player Settings
```
Company Name: Your Company
Product Name: Hungry Shark Evolution
Bundle Identifier: com.yourcompany.hungryshark
Version: 1.0.0
```

## Getting Started

1. **Open Project**: Open the folder in Unity Hub
2. **Import Assets**: Import required 3D models and textures
3. **Setup Scene**: Configure the MainGame scene with required GameObjects
4. **Build Settings**: Configure platform-specific settings
5. **Test Play**: Test in editor before building

## Troubleshooting

### Common Issues
- **Performance**: Reduce particle effects and fish count
- **Touch Input**: Verify touch sensitivity settings
- **Physics**: Adjust rigidbody drag for realistic underwater movement
- **UI Scaling**: Use Canvas Scaler with "Scale With Screen Size"

### Debug Features
- Use Gizmos to visualize detection ranges and boundaries
- Enable Debug.Log statements in development builds
- Use Unity Profiler for performance optimization