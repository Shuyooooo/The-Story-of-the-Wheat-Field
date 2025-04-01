using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for potential scene validation later
using System.Linq; // Needed for LINQ queries like FirstOrDefault, GroupBy

#if UNITY_EDITOR
using UnityEditor; // Needed for editor-specific checks like asset database access
#endif

/// <summary>
/// Defines different types of transitions between scenes.
/// </summary>
[System.Serializable] // Ensure enum values are saved correctly with the SO
public enum TransitionType
{
    Fade,
    Wipe,
    Instant,
    Custom // For potentially complex, script-driven transitions
}

/// <summary>
/// Represents a single route between two points (scenes or locations within scenes).
/// NOTE: You might want to make From/To more specific, e.g., using scene names,
/// build indices, or custom location identifiers within scenes. Using strings for now.
/// </summary>
[System.Serializable] // Make it serializable so Unity can save it within the ScriptableObject list
public class SceneRoute
{
    [Tooltip("Unique identifier for this route. Helps with lookups and debugging.")]
    public string routeID = System.Guid.NewGuid().ToString(); // Auto-generate a unique ID

    [Tooltip("Description of the route for editor clarity.")]
    public string description = "New Scene Route";

    [Tooltip("The identifier (e.g., scene name or custom location ID) where this route originates.")]
    public string fromSceneIdentifier;

    [Tooltip("The identifier (e.g., scene name or custom location ID) where this route leads.")]
    public string toSceneIdentifier;

    [Tooltip("Is this route currently active and usable?")]
    public bool isEnabled = true;

    [Tooltip("Type of visual transition to use.")]
    public TransitionType transitionType = TransitionType.Fade;

    [Tooltip("Minimum player level required (Example condition).")]
    public int requiredLevel = 0;

    [Tooltip("Item required in inventory (Example condition - use item ID or ScriptableObject reference).")]
    public string requiredItemID = "";

    [Tooltip("Quest that must be completed (Example condition - use quest ID or ScriptableObject reference).")]
    public string requiredQuestID = "";

    // Add more complex conditions if needed, potentially using a dedicated Condition System (ScriptableObjects?)
    // public List<ConditionBaseSO> conditions;

    [Tooltip("Optional data associated with this route (e.g., specific spawn point ID in the destination).")]
    public string customData = "";

    /// <summary>
    /// Basic check if the route *might* be usable based on simple conditions.
    /// A full check would likely involve querying game state managers (player level, inventory, quest status).
    /// </summary>
    /// <param name="playerLevel">Current player level.</param>
    /// <param name="inventory">Reference to player inventory system.</param>
    /// <param name="questLog">Reference to player quest log.</param>
    /// <returns>True if basic conditions met, false otherwise.</returns>
    public bool AreConditionsMet(int playerLevel /*, PlayerInventory inventory, QuestLog questLog */)
    {
        if (!isEnabled) return false;

        bool levelMet = playerLevel >= requiredLevel;
        // bool itemMet = string.IsNullOrEmpty(requiredItemID) || inventory.HasItem(requiredItemID);
        // bool questMet = string.IsNullOrEmpty(requiredQuestID) || questLog.IsQuestComplete(requiredQuestID);

        // Combine all conditions
        // return levelMet && itemMet && questMet;
        return levelMet; // Simplified for example
    }
}

/// <summary>
/// ScriptableObject to store and manage all possible scene routes in the game.
/// Provides methods for accessing and validating route data.
/// </summary>
[CreateAssetMenu(fileName = "SceneRouteDataList_SO", menuName = "Map/Scene Route Data List")]
public class SceneRouteDataList_SO : ScriptableObject
{
    [Tooltip("The master list of all defined scene routes.")]
    public List<SceneRoute> sceneRouteList = new List<SceneRoute>();

    // --- Runtime Dictionaries for Fast Lookups ---
    // Populated during initialization (e.g., OnEnable or first access)
    private Dictionary<string, SceneRoute> routeLookupByID = new Dictionary<string, SceneRoute>();
    // Key: Tuple(FromScene, ToScene) -> Value: SceneRoute
    private Dictionary<(string, string), SceneRoute> routeLookupByFromTo = new Dictionary<(string, string), SceneRoute>();
    // Key: FromScene -> Value: List of routes originating from that scene
    private Dictionary<string, List<SceneRoute>> routesLookupByFrom = new Dictionary<string, List<SceneRoute>>();

    private bool isInitialized = false;

    /// <summary>
    /// Initializes the lookup dictionaries for faster runtime access.
    /// Call this before using any lookup methods, potentially from a game manager on startup.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // Clear existing dictionaries to handle potential re-initialization
        routeLookupByID.Clear();
        routeLookupByFromTo.Clear();
        routesLookupByFrom.Clear();

        foreach (var route in sceneRouteList)
        {
            if (route == null)
            {
                Debug.LogWarning($"[{this.name}] Found a null entry in sceneRouteList. Skipping.");
                continue;
            }

            // Populate ID lookup
            if (!string.IsNullOrEmpty(route.routeID))
            {
                if (!routeLookupByID.ContainsKey(route.routeID))
                {
                    routeLookupByID.Add(route.routeID, route);
                }
                else
                {
                    Debug.LogWarning($"[{this.name}] Duplicate Route ID found: '{route.routeID}'. Only the first instance will be used in ID lookup.");
                }
            }
            else
            {
                 Debug.LogWarning($"[{this.name}] Route with empty ID found (From: {route.fromSceneIdentifier}, To: {route.toSceneIdentifier}). Cannot add to ID lookup.");
            }


            // Populate From->To lookup
            if (!string.IsNullOrEmpty(route.fromSceneIdentifier) && !string.IsNullOrEmpty(route.toSceneIdentifier))
            {
                var key = (route.fromSceneIdentifier, route.toSceneIdentifier);
                if (!routeLookupByFromTo.ContainsKey(key))
                {
                    routeLookupByFromTo.Add(key, route);
                }
                else
                {
                    Debug.LogWarning($"[{this.name}] Duplicate From->To route found: From '{route.fromSceneIdentifier}' To '{route.toSceneIdentifier}'. Only the first instance will be used in From->To lookup.");
                }
            }
             else
            {
                 Debug.LogWarning($"[{this.name}] Route with empty From or To Identifier found (ID: {route.routeID}). Cannot add to From->To lookup.");
            }

            // Populate From lookup
            if (!string.IsNullOrEmpty(route.fromSceneIdentifier))
            {
                if (!routesLookupByFrom.ContainsKey(route.fromSceneIdentifier))
                {
                    routesLookupByFrom.Add(route.fromSceneIdentifier, new List<SceneRoute>());
                }
                routesLookupByFrom[route.fromSceneIdentifier].Add(route);
            }
        }

        isInitialized = true;
        Debug.Log($"[{this.name}] SceneRouteDataList Initialized. Found {sceneRouteList.Count} routes. Populated lookups.");
    }

    // --- Accessor Methods ---

    /// <summary>
    /// Gets a specific route by its unique ID. Requires Initialize() to be called first.
    /// </summary>
    /// <param name="routeID">The ID of the route to find.</param>
    /// <returns>The SceneRoute if found, otherwise null.</returns>
    public SceneRoute GetRouteByID(string routeID)
    {
        if (!isInitialized) {
             Debug.LogError($"[{this.name}] Attempted to GetRouteByID before initialization!");
             Initialize(); // Or consider throwing an exception
        }
        routeLookupByID.TryGetValue(routeID, out SceneRoute route);
        return route;
    }

    /// <summary>
    /// Finds a specific route based on the origin and destination identifiers. Requires Initialize() to be called first.
    /// </summary>
    /// <param name="fromSceneIdentifier">Origin identifier.</param>
    /// <param name="toSceneIdentifier">Destination identifier.</param>
    /// <returns>The SceneRoute if found, otherwise null.</returns>
    public SceneRoute FindRoute(string fromSceneIdentifier, string toSceneIdentifier)
    {
        if (!isInitialized) {
             Debug.LogError($"[{this.name}] Attempted to FindRoute before initialization!");
              Initialize(); // Or consider throwing an exception
        }
        var key = (fromSceneIdentifier, toSceneIdentifier);
        routeLookupByFromTo.TryGetValue(key, out SceneRoute route);
        return route;
    }

    /// <summary>
    /// Gets all routes originating from a specific scene/location identifier. Requires Initialize() to be called first.
    /// </summary>
    /// <param name="fromSceneIdentifier">Origin identifier.</param>
    /// <returns>A list of SceneRoutes originating from the specified location, or an empty list if none found.</returns>
    public List<SceneRoute> GetRoutesFrom(string fromSceneIdentifier)
    {
         if (!isInitialized) {
             Debug.LogError($"[{this.name}] Attempted to GetRoutesFrom before initialization!");
              Initialize(); // Or consider throwing an exception
        }
        if (routesLookupByFrom.TryGetValue(fromSceneIdentifier, out List<SceneRoute> routes))
        {
            return routes; // Returns the list from the dictionary
        }
        return new List<SceneRoute>(); // Return an empty list if no routes found from this origin
    }

     /// <summary>
    /// Gets all defined routes. Returns a read-only view of the list.
    /// </summary>
    public IReadOnlyList<SceneRoute> GetAllRoutes()
    {
        // Provides read-only access to prevent external modification of the master list
        return sceneRouteList.AsReadOnly();
    }

    /// <summary>
    /// Finds all routes that lead *to* a specific scene/location identifier.
    /// This uses LINQ on the main list, so it might be less performant than dictionary lookups if called very frequently.
    /// Consider adding a dedicated `routesLookupByTo` dictionary if needed.
    /// </summary>
    /// <param name="toSceneIdentifier">Destination identifier.</param>
    /// <returns>An enumerable collection of routes leading to the destination.</returns>
    public IEnumerable<SceneRoute> GetRoutesTo(string toSceneIdentifier)
    {
        // LINQ query to find matching routes
        return sceneRouteList.Where(route => route != null && route.toSceneIdentifier == toSceneIdentifier);
    }


    // --- Editor Validation ---

#if UNITY_EDITOR
    /// <summary>
    /// Called in the editor when the script is loaded or a value is changed in the Inspector.
    /// Use this for validation checks.
    /// </summary>
    private void OnValidate()
    {
        if (sceneRouteList == null) {
             sceneRouteList = new List<SceneRoute>();
        }

        // 1. Check for null entries
        int nullCount = sceneRouteList.Count(r => r == null);
        if (nullCount > 0)
        {
            Debug.LogWarning($"[{this.name}] Contains {nullCount} null entries in sceneRouteList. Please remove them.", this);
            // Optionally remove them automatically:
            // sceneRouteList.RemoveAll(r => r == null);
        }

        // 2. Check for duplicate Route IDs
        var idGroups = sceneRouteList.Where(r => r != null && !string.IsNullOrEmpty(r.routeID)) // Filter valid routes
                                     .GroupBy(r => r.routeID)        // Group by ID
                                     .Where(g => g.Count() > 1);   // Find groups with more than one member

        foreach (var group in idGroups)
        {
            Debug.LogWarning($"[{this.name}] Duplicate Route ID '{group.Key}' found {group.Count()} times. Route IDs should be unique.", this);
            // Highlight the problem routes? Difficult without a custom editor.
        }

        // 3. Check for duplicate From->To pairs
        var fromToGroups = sceneRouteList.Where(r => r != null && !string.IsNullOrEmpty(r.fromSceneIdentifier) && !string.IsNullOrEmpty(r.toSceneIdentifier))
                                        .GroupBy(r => (r.fromSceneIdentifier, r.toSceneIdentifier))
                                        .Where(g => g.Count() > 1);

        foreach (var group in fromToGroups)
        {
             Debug.LogWarning($"[{this.name}] Duplicate route definition found: From '{group.Key.Item1}' To '{group.Key.Item2}' ({group.Count()} times).", this);
        }


        // 4. Check for empty identifiers (optional, but good practice)
        foreach(var route in sceneRouteList) {
            if (route == null) continue;
            if (string.IsNullOrWhiteSpace(route.routeID)) {
                 Debug.LogWarning($"[{this.name}] Route from '{route.fromSceneIdentifier}' to '{route.toSceneIdentifier}' has an empty or whitespace Route ID.", this);
            }
             if (string.IsNullOrWhiteSpace(route.fromSceneIdentifier)) {
                 Debug.LogWarning($"[{this.name}] Route with ID '{route.routeID}' has an empty or whitespace 'From Scene Identifier'.", this);
            }
             if (string.IsNullOrWhiteSpace(route.toSceneIdentifier)) {
                 Debug.LogWarning($"[{this.name}] Route with ID '{route.routeID}' has an empty or whitespace 'To Scene Identifier'.", this);
            }
        }

        // Potential Future Validation:
        // - Check if scene names in from/to identifiers actually exist in Build Settings.
        //   (Can be slow, might require EditorBuildSettings.scenes)
        // - Check if requiredItemIDs / requiredQuestIDs correspond to valid game data assets.

        // Reset initialization flag if data changes in editor
        isInitialized = false;
    }
#endif

    // --- Utility Functions (Optional) ---

    /// <summary>
    /// Adds a new route to the list and attempts to update the dictionaries if initialized.
    /// Primarily for runtime modification, use with caution. Editor modification is generally preferred.
    /// </summary>
    /// <param name="newRoute">The SceneRoute to add.</param>
    /// <returns>True if added successfully, false otherwise (e.g., null route).</returns>
    public bool AddRoute(SceneRoute newRoute)
    {
        if (newRoute == null) return false;

        // Prevent adding exact duplicates based on reference (or implement deeper comparison if needed)
        if (sceneRouteList.Contains(newRoute)) return false;

        sceneRouteList.Add(newRoute);

        // Update dictionaries if already initialized
        if (isInitialized)
        {
             // Add to ID lookup (with duplicate check)
            if (!string.IsNullOrEmpty(newRoute.routeID) && !routeLookupByID.ContainsKey(newRoute.routeID)) {
                 routeLookupByID.Add(newRoute.routeID, newRoute);
            }

             // Add to From->To lookup (with duplicate check)
             if (!string.IsNullOrEmpty(newRoute.fromSceneIdentifier) && !string.IsNullOrEmpty(newRoute.toSceneIdentifier)) {
                 var key = (newRoute.fromSceneIdentifier, newRoute.toSceneIdentifier);
                 if (!routeLookupByFromTo.ContainsKey(key)) {
                    routeLookupByFromTo.Add(key, newRoute);
                 }
             }


            // Add to From lookup
            if (!string.IsNullOrEmpty(newRoute.fromSceneIdentifier))
            {
                if (!routesLookupByFrom.ContainsKey(newRoute.fromSceneIdentifier))
                {
                    routesLookupByFrom.Add(newRoute.fromSceneIdentifier, new List<SceneRoute>());
                }
                // Avoid adding the same route object multiple times if logic allows it
                if(!routesLookupByFrom[newRoute.fromSceneIdentifier].Contains(newRoute)) {
                     routesLookupByFrom[newRoute.fromSceneIdentifier].Add(newRoute);
                }
            }
        } else {
            // If not initialized, adding to the list is enough.
            // The next Initialize() call will pick it up.
        }
        return true;
    }

     /// <summary>
    /// Removes a route using its ID. Also updates lookup dictionaries if initialized.
    /// Primarily for runtime modification, use with caution.
    /// </summary>
    /// <param name="routeID">The ID of the route to remove.</param>
    /// <returns>True if a route was found and removed, false otherwise.</returns>
    public bool RemoveRoute(string routeID)
    {
        SceneRoute routeToRemove = GetRouteByID(routeID); // Use existing lookup if initialized
        if(routeToRemove == null && !isInitialized) {
            // If not initialized, fall back to searching the list manually
             routeToRemove = sceneRouteList.FirstOrDefault(r => r != null && r.routeID == routeID);
        }


        if (routeToRemove != null)
        {
            bool removedFromList = sceneRouteList.Remove(routeToRemove);

            if(removedFromList && isInitialized) {
                 // Remove from dictionaries
                 if (!string.IsNullOrEmpty(routeToRemove.routeID)) {
                     routeLookupByID.Remove(routeToRemove.routeID);
                 }

                 if (!string.IsNullOrEmpty(routeToRemove.fromSceneIdentifier) && !string.IsNullOrEmpty(routeToRemove.toSceneIdentifier)) {
                     routeLookupByFromTo.Remove((routeToRemove.fromSceneIdentifier, routeToRemove.toSceneIdentifier));
                 }

                 if (!string.IsNullOrEmpty(routeToRemove.fromSceneIdentifier) && routesLookupByFrom.ContainsKey(routeToRemove.fromSceneIdentifier)) {
                     routesLookupByFrom[routeToRemove.fromSceneIdentifier].Remove(routeToRemove);
                     // Optional: Remove the key entirely if the list becomes empty
                     if (routesLookupByFrom[routeToRemove.fromSceneIdentifier].Count == 0) {
                         routesLookupByFrom.Remove(routeToRemove.fromSceneIdentifier);
                     }
                 }
            }
            return removedFromList;
        }
        return false;
    }
}
