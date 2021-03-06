﻿using Pokemon3D.DataModel;
using Pokemon3D.DataModel.GameMode.Battle;
using Pokemon3D.DataModel.GameMode.Pokemon;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Linq;

namespace Pokemon3D.Editor.Core.Model
{
    public class GameModeModel
    {
        private const string FolderNameContent = "Content";
        private const string FolderNameTextures = "Textures";
        private const string FolderNameModels = "Models";

        private const string FolderNameData = "Data";
        private const string FolderNameMoves = "Moves";
        private const string FolderNamePokemon = "Pokemon";

        private const string FolderNameFragments = "Fragments";

        private const string FolderNameMaps = "Maps";

        private const string FolderNameScripts = "Scripts";

        public const string GameModeJsonFile = "GameMode.json";

        private readonly List<TextureModel> _textureModels;
        private readonly List<ModelModel> _modelModels;
        private readonly List<MoveModel> _moveModels;
        private readonly List<PokemonModel> _pokemonModel;

        public GameModeModel()
        {
            _textureModels = new List<TextureModel>();
            TextureModels = _textureModels.AsReadOnly();

            _modelModels = new List<ModelModel>();
            ModelModels = _modelModels.AsReadOnly();

            _moveModels = new List<MoveModel>();
            MoveModels = _moveModels.AsReadOnly();

            _pokemonModel = new List<PokemonModel>();
            PokemonModels = _pokemonModel.AsReadOnly();
        }

        public static GameModeModel Create(string folderPath)
        {
            EnsureDefaultFoldersExists(folderPath);

            return new GameModeModel();
        }

        public static GameModeModel Open(string folderPath)
        {
            EnsureDefaultFoldersExists(folderPath);

            var gameMode = new GameModeModel();

            var gameModeFilePath = Path.Combine(folderPath, GameModeJsonFile);
            gameMode.GameModeDataModel = DataModel<DataModel.GameMode.GameModeModel>.FromFile(gameModeFilePath);

            ReadContentFolder(gameMode, folderPath);
            ReadDataFolder(gameMode, folderPath);

            return gameMode;
        }
        
        private static void ReadContentFolder(GameModeModel model, string folderPath)
        {
            var texturesPath = Path.Combine(folderPath, FolderNameContent, FolderNameTextures);
            FileSystem.GetFilesRecursive(texturesPath, file => model.AddTexture(new TextureModel(texturesPath, file)));

            var modelPath = Path.Combine(folderPath, FolderNameContent, FolderNameModels);
            FileSystem.GetFilesOfFolderRecursive(modelPath, files =>
            {
                if (files.Any(f => (Path.GetExtension(f) ?? "").Equals(".obj", StringComparison.OrdinalIgnoreCase)))
                {
                    model.AddModel(new ModelModel(modelPath, files));
                }
            });
        }

        private static void ReadDataFolder(GameModeModel gameMode, string folderPath)
        {
            FileSystem.GetFiles(Path.Combine(folderPath, FolderNameData, FolderNameMoves), f => gameMode.AddMove(DataModel<MoveModel>.FromFile(f)));
            FileSystem.GetFiles(Path.Combine(folderPath, FolderNameData, FolderNamePokemon), f => gameMode.AddPokemon(DataModel<PokemonModel>.FromFile(f)));
        }

        private static void EnsureDefaultFoldersExists(string folderPath)
        {
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameContent));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameContent, FolderNameTextures));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameContent, FolderNameModels));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameData));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameData, FolderNameMoves));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameData, FolderNamePokemon));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameFragments));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameMaps));
            FileSystem.EnsureFolderExists(Path.Combine(folderPath, FolderNameScripts));
        }

        public ReadOnlyCollection<TextureModel> TextureModels { get; private set; }

        public ReadOnlyCollection<ModelModel> ModelModels { get; private set; }

        public ReadOnlyCollection<MoveModel> MoveModels { get; private set; }

        public ReadOnlyCollection<PokemonModel> PokemonModels { get; private set; }

        public DataModel.GameMode.GameModeModel GameModeDataModel { get; private set; }

        public void AddTexture(TextureModel textureModel)
        {
            _textureModels.Add(textureModel);
        }

        public void AddModel(ModelModel model)
        {
            _modelModels.Add(model);
        }

        public void AddMove(MoveModel model)
        {
            _moveModels.Add(model);
        }

        public void AddPokemon(PokemonModel model)
        {
            _pokemonModel.Add(model);
        }
    }
}