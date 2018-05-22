﻿namespace WorldBuilder
{
    using Zenject;

    public class AppInstaller : ScriptableObjectInstaller
    {
        public string MaterialsAsset = "materials_3.xml";

        public override void InstallBindings()
        {
            Container.Bind<AssetManager>().FromNew().AsSingle().NonLazy();
            Container.Bind<MaterialDefinitionManager>().FromNew().AsSingle().WithArguments(MaterialsAsset).NonLazy();
        }
    }
}