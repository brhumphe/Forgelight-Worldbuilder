﻿namespace WorldBuilder.Objects
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Formats.Adr;
    using Formats.Dme;
    using Materials;
    using Meshes;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Zenject;

    /// <summary>
    /// Handles creation, and destruction of actor instances.
    /// </summary>
    public class ActorFactory
    {
        // Dependencies
        [Inject] private AssetManager assetManager;
        [Inject] private ActorDefinitionManager actorDefinitionManager;
        [Inject] private ActorMaterialFactory materialFactory;
        [Inject] private ActorMeshFactory actorMeshFactory;

        private Dictionary<string, ForgelightActor> cachedActors = new Dictionary<string, ForgelightActor>();
        private GameObject actorPoolParent;

        private ForgelightActor missingActorPrefab;

        [Inject]
        public ActorFactory(GameManager gameManager, ForgelightActor missingActorPrefab)
        {
            this.missingActorPrefab = missingActorPrefab;
            gameManager.OnGameLoaded += OnGameLoaded;
        }

        private void OnGameLoaded()
        {
            if (actorPoolParent != null)
            {
                Object.Destroy(actorPoolParent);
            }

            // TODO Object Pooling
            cachedActors.Clear();
            actorPoolParent = new GameObject("Available Actors");
            actorPoolParent.SetActive(false);
        }

        public async Task<ForgelightActor> CreateActor(string actorDefName)
        {
            Adr actorDefinition = actorDefinitionManager.GetDefinition(actorDefName);

            if (actorDefinition != null)
            {
                return await CreateActor(actorDefinition);
            }

            Debug.LogWarning("Actor \"" + actorDefName + "\" does not exist!");
            return null;
        }

        public async Task<ForgelightActor> CreateActor(Adr actorDefinition)
        {
            ForgelightActor actorSource;
            if (!cachedActors.TryGetValue(actorDefinition.Name, out actorSource))
            {
                actorSource = await LoadNewActor(actorDefinition);
                cachedActors[actorDefinition.Name] = actorSource;
            }

            ForgelightActor actorInstance = Object.Instantiate(actorSource);

            return actorInstance;
        }

        /// <summary>
        /// Attempts to find a compatible DME associated with the actor definition.
        /// </summary>
        private Dme GetActorDme(Adr actorDefinition)
        {
            Dme modelDef = assetManager.LoadPackAsset<Dme>(actorDefinition.Base);
            if (modelDef != null)
            {
                return modelDef;
            }

            Debug.LogWarningFormat("Could not use base model for actor \"{0}\". Attempting to use LOD model instead.", actorDefinition.DisplayName);
            foreach (Lod lod in actorDefinition.Lods)
            {
                modelDef = assetManager.LoadPackAsset<Dme>(lod.FileName);
                if (modelDef != null)
                {
                    break;
                }
            }

            return modelDef;
        }

        private async Task<ForgelightActor> LoadNewActor(Adr actorDefinition)
        {
            await new WaitForBackgroundThread();

            ForgelightActor actor;

            // Attempt to find the model's Dme.
            Dme modelDef = GetActorDme(actorDefinition);

            if (modelDef == null)
            {
                actor = CreateMissingPlaceholder(actorDefinition);
            }
            else
            {
                // Deserialization
                MeshData meshData = actorMeshFactory.GenerateMeshData(modelDef);

                await new WaitForUpdate();

                UnityEngine.Mesh mesh = actorMeshFactory.CreateMeshFromData(modelDef.Name, meshData);
                Material[] materials = materialFactory.GetActorMaterials(modelDef);

                assetManager.Dispose(modelDef);

                Assert.IsNotNull(mesh);
                Assert.IsNotNull(materials);

                // GameObject "Prefab"
                GameObject actorSource = new GameObject();
                actor = actorSource.AddComponent<ForgelightActor>();
                MeshFilter meshFilter = actorSource.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = actorSource.AddComponent<MeshRenderer>();

                // Assign deserialized data
                meshFilter.sharedMesh = mesh;
                meshRenderer.sharedMaterials = materials;
            }

            // TODO LOD Groups
            actor.name = actorDefinition.DisplayName;
            actor.Init(actorDefinition);
            actor.transform.SetParent(actorPoolParent.transform);

            return actor;
        }

        private ForgelightActor CreateMissingPlaceholder(Adr actorDefinition)
        {
            Debug.LogErrorFormat("Could not find mesh data for actor definition \"{0}\". Using placeholder asset.", actorDefinition.DisplayName);
            return Object.Instantiate(missingActorPrefab);
        }
    }
}