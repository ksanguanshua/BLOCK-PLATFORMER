using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for Linq operations like FirstOrDefault

public class ParticleManager : MonoBehaviour
{
    [System.Serializable]
    public class ParticleSystemEntry
    {
        public string name;
        public ParticleSystem particleSystem;
    }
    [System.Serializable]
    public class ParticleSystemPopEntry
    {
        public string name;
        public GameObject particleSystem;
    }

    [Tooltip("Drag your Particle System GameObjects here or allow the script to find them as children.")]
    public List<ParticleSystemEntry> particleSystems = new List<ParticleSystemEntry>();

    [Tooltip("Drag your Particle System GameObjects here or allow the script to find them as children.")]
    public List<ParticleSystemPopEntry> particlePopSystems = new List<ParticleSystemPopEntry>();

    [Tooltip("If true, the script will automatically try to find and add Particle Systems from child GameObjects if their names are not already in the list.")]
    public bool autoPopulateFromChildren = true;

    [Tooltip("If auto-populating, only add child particle systems whose names start with this prefix. Leave empty to include all.")]
    public string particleChildPrefix = "";

    void Awake()
    {
        if (autoPopulateFromChildren)
        {
            PopulateParticleSystemsFromChildren();
        }

        // Ensure all particle systems are initially stopped
        foreach (var entry in particleSystems)
        {
            if (entry.particleSystem != null)
            {
                entry.particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                Debug.LogWarning($"Particle system for entry '{entry.name}' is not assigned in '{gameObject.name}'.");
            }
        }
    }

    /// <summary>
    /// Automatically finds ParticleSystems in children and adds them to the list
    /// if they are not already present and match the optional prefix.
    /// </summary>
    void PopulateParticleSystemsFromChildren()
    {
        ParticleSystem[] childParticleSystems = GetComponentsInChildren<ParticleSystem>(true); // true to include inactive

        foreach (ParticleSystem ps in childParticleSystems)
        {
            // Skip if this particle system is already manually added (by reference)
            if (particleSystems.Any(entry => entry.particleSystem == ps))
            {
                continue;
            }

            // Skip if a particle system with the same GameObject name is already in the list (by name)
            if (particleSystems.Any(entry => entry.name == ps.gameObject.name))
            {
                // If you want to update the reference if it's null, you could add logic here
                // For now, we assume if a named entry exists, it's intentional
                continue;
            }

            // Filter by prefix if a prefix is specified
            if (!string.IsNullOrEmpty(particleChildPrefix) && !ps.gameObject.name.StartsWith(particleChildPrefix))
            {
                continue;
            }

            // Add new entry
            particleSystems.Add(new ParticleSystemEntry { name = ps.gameObject.name, particleSystem = ps });
            Debug.Log($"Automatically added particle system: {ps.gameObject.name} to ParticleManager on {gameObject.name}");
        }
    }

    /// <summary>
    /// Plays a particle system from the list by its assigned name.
    /// </summary>
    /// <param name="particleName">The name of the particle system to play.</param>
    public void PlayPopParticle(string particleName, Vector2 pos)
    {
        ParticleSystemPopEntry entry = particlePopSystems.FirstOrDefault(p => p.name == particleName);

        if (entry != null && entry.particleSystem != null)
        {
            Instantiate(entry.particleSystem,pos,Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Particle system with name '{particleName}' not found or not assigned in ParticleManager on '{gameObject.name}'.");
        }
    }


    /// <summary>
    /// Plays a particle system from the list by its assigned name.
    /// </summary>
    /// <param name="particleName">The name of the particle system to play.</param>
    public void PlayParticle(string particleName)
    {
        ParticleSystemEntry entry = particleSystems.FirstOrDefault(p => p.name == particleName);

        if (entry != null && entry.particleSystem != null)
        {
            entry.particleSystem.Play();
        }
        else
        {
            Debug.LogWarning($"Particle system with name '{particleName}' not found or not assigned in ParticleManager on '{gameObject.name}'.");
        }
    }

    /// <summary>
    /// Stops a particle system from the list by its assigned name.
    /// </summary>
    /// <param name="particleName">The name of the particle system to stop.</param>
    /// <param name="withChildren">Should child particle systems also be stopped?</param>
    /// <param name="stopBehavior">How the particle system should stop.</param>
    public void StopParticle(string particleName, bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
    {
        ParticleSystemEntry entry = particleSystems.FirstOrDefault(p => p.name == particleName);

        if (entry != null && entry.particleSystem != null)
        {
            entry.particleSystem.Stop(withChildren, stopBehavior);
        }
        else
        {
            Debug.LogWarning($"Particle system with name '{particleName}' not found or not assigned in ParticleManager on '{gameObject.name}'.");
        }
    }

    /// <summary>
    /// Gets a specific ParticleSystem by name.
    /// </summary>
    /// <param name="particleName">The name of the particle system to get.</param>
    /// <returns>The ParticleSystem component or null if not found.</returns>
    public ParticleSystem GetParticleSystem(string particleName)
    {
        ParticleSystemEntry entry = particleSystems.FirstOrDefault(p => p.name == particleName);
        if (entry != null)
        {
            return entry.particleSystem;
        }
        Debug.LogWarning($"Particle system with name '{particleName}' not found in ParticleManager on '{gameObject.name}'.");
        return null;
    }

    // Optional: Add a method to add particle systems at runtime if needed
    public void AddParticleSystem(string name, ParticleSystem ps)
    {
        if (particleSystems.Any(p => p.name == name))
        {
            Debug.LogWarning($"Particle system with name '{name}' already exists in ParticleManager on '{gameObject.name}'. Cannot add.");
            return;
        }
        particleSystems.Add(new ParticleSystemEntry { name = name, particleSystem = ps });
    }
    /// <summary>
    /// Plays a particle system from the list by its assigned name ONLY if it is not currently playing.
    /// </summary>
    /// <param name="particleName">The name of the particle system to play.</param>
    public void StartPlay(string particleName)
    {
        ParticleSystemEntry entry = particleSystems.FirstOrDefault(p => p.name == particleName);

        if (entry != null && entry.particleSystem != null)
        {
            if (!entry.particleSystem.isPlaying) // Check if the particle system is not already playing
            {
                entry.particleSystem.Play();
            }
            // Optional: Add a log if you want to know it was already playing
            // else
            // {
            //     Debug.Log($"Particle system '{particleName}' on '{gameObject.name}' is already playing.");
            // }
        }
        else
        {
            Debug.LogWarning($"Particle system with name '{particleName}' not found or not assigned for StartPlay in ParticleManager on '{gameObject.name}'.");
        }
    }
}