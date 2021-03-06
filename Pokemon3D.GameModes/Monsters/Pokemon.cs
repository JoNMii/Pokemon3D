﻿using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon3D.Common;
using Pokemon3D.DataModel.GameMode.Battle;
using Pokemon3D.DataModel.GameMode.Definitions;
using Pokemon3D.DataModel.GameMode.Pokemon;
using Pokemon3D.DataModel.Pokemon;
using Pokemon3D.DataModel.Savegame.Pokemon;

namespace Pokemon3D.GameModes.Monsters
{
    /// <summary>
    /// A Pokémon instance that holds the definition and save data of a Pokémon instance.
    /// </summary>
    public class Pokemon
    {
        private const string DEFAULT_FORM_ID = "Default";
        private const string SHINY_FORM_ID = "Shiny";
        private const int POKEMON_MAX_LEVEL = 100;
        private const int POKEMON_MAX_MOVE_COUNT = 4;

        private GameMode _gameMode;

        private PokemonModel _dataModel;
        private PokemonSaveModel _saveModel;

        #region Data Model Properties

        /// <summary>
        /// The effort values of this Pokémon.
        /// </summary>
        /// <remarks>http://bulbapedia.bulbagarden.net/wiki/Effort_values</remarks>
        public PokemonStatSetModel EVs => _saveModel.EVs;

        /// <summary>
        /// The individual values of this Pokémon.
        /// </summary>
        /// <remarks>http://bulbapedia.bulbagarden.net/wiki/Individual_values</remarks>
        public PokemonStatSetModel IVs => _saveModel.IVs;

        /// <summary>
        /// The base stats of this Pokémon.
        /// </summary>
        /// <remarks>http://bulbapedia.bulbagarden.net/wiki/Statistic</remarks>
        public PokemonStatSetModel BaseStats => ActiveFormModel.BaseStats;

        #region Stats

        /// <summary>
        /// The current health points of this Pokémon.
        /// </summary>
        public int HP
        {
            get { return _saveModel.HP; }
            set { _saveModel.HP = value; }
        }

        public int MaxHP => PokemonStatCalculator.CalculateStat(this, PokemonStatType.HP);

        public int Attack => PokemonStatCalculator.CalculateStat(this, PokemonStatType.Attack);

        public int Defense => PokemonStatCalculator.CalculateStat(this, PokemonStatType.Defense);

        public int SpecialAttack => PokemonStatCalculator.CalculateStat(this, PokemonStatType.SpecialAttack);

        public int SpecialDefense => PokemonStatCalculator.CalculateStat(this, PokemonStatType.SpecialDefense);

        public int Speed => PokemonStatCalculator.CalculateStat(this, PokemonStatType.Speed);

        #endregion

        public int Experience
        {
            get { return _saveModel.Experience; }
            set { _saveModel.Experience = value; }
        }

        public int Level
        {
            get
            {
                return PokemonExperienceCalculator.LevelForExperienceValue(_dataModel.ExperienceType, Experience);
            }
            set
            {
                // when the level is set from outside, the experience is set to exactly reach that level.
                int experienceNeeded = PokemonExperienceCalculator.ExperienceNeededForLevel(_dataModel.ExperienceType, value);
                Experience = experienceNeeded;
            }
        }

        public AbilityModel Ability
        {
            get
            {
                if (_saveModel.AbilityIndex == 0)
                    _saveModel.AbilityIndex = GlobalRandomProvider.Instance.Rnd.Next(0, ActiveFormModel.Abilities.Length);

                string id;
                if (ActiveFormModel.Abilities.Length <= _saveModel.AbilityIndex)
                    id = ActiveFormModel.Abilities.Last();
                else
                    id = ActiveFormModel.Abilities[_saveModel.AbilityIndex];

                return _gameMode.GetAbilityModel(id);
            }
        }

        public NatureModel Nature => _gameMode.GetNatureModel(_saveModel.NatureId);

        public TypeModel Type1 => _gameMode.GetTypeModel(ActiveFormModel.Type1);

        public TypeModel Type2 => _gameMode.GetTypeModel(ActiveFormModel.Type2);

        public string DisplayName => !string.IsNullOrWhiteSpace(_saveModel.Nickname) ? _saveModel.Nickname : _dataModel.Name;

        public LevelUpMoveModel[] LevelMoves => GetMovePoolModel(ActiveFormModel).LevelMoves;

        public string[] LearnableMoves => GetMovePoolModel(ActiveFormModel).LearnableMoves;

        public PokemonFormModel ActiveFormModel
        {
            get
            {
                string activeForm = GetActiveFormId();
                return _dataModel.Forms.Single(x => x.Id == activeForm);
            }
        }

        /// <summary>
        /// Returns the model that is responsible for the move pools for a form model.
        /// </summary>
        private PokemonFormModel GetMovePoolModel(PokemonFormModel formModel)
        {
            if (!string.IsNullOrWhiteSpace(formModel.ShareMovesWithForm) && _dataModel.Forms.Any(f => f.Id == formModel.ShareMovesWithForm))
            {
                return GetMovePoolModel(_dataModel.Forms.Single(f => f.Id == formModel.ShareMovesWithForm));
            }
            else
            {
                return formModel;
            }
        }

        #endregion

        #region Textures

        private const string KEY_FORMAT = "{0}\\{1}\\{2}";

        private Texture2D GetMenuTexture(string pokemonId, string formId, string textureSource)
        {
            var key = string.Format(KEY_FORMAT, pokemonId, formId, textureSource);

            return _gameMode.GetTexture(key);
        }

        public Texture2D GetMenuTexture()
        {
            return GetMenuTexture(_dataModel.Id, ActiveFormModel.Id, ActiveFormModel.MenuTexture.Source);
        }

        #endregion

        public Pokemon(GameMode gameMode, PokemonModel dataModel, PokemonSaveModel saveModel)
        {
            _gameMode = gameMode;

            _dataModel = dataModel;
            _saveModel = saveModel;
        }

        private string GetActiveFormId()
        {
            if (_dataModel.Forms.Length == 1)
                return _dataModel.Forms.First().Id;

            // determine the current form of the Pokémon here.
            // if no special form applies, set to "Default".

            if (_saveModel.IsShiny && _dataModel.Forms.Any(f => f.Id == SHINY_FORM_ID))
            {
                return SHINY_FORM_ID;
            }
            else
            {
                return DEFAULT_FORM_ID;
            }
        }

        /// <summary>
        /// Makes the Pokémon learn all moves that are learnt on Level 1.
        /// </summary>
        public void LearnStartupMoves()
        {
            LearnMove(1);
        }

        /// <summary>
        /// Does level up progress for this Pokémon.
        /// </summary>
        public void LevelUp(bool learnRandomMove, int levels = 1)
        {
            // target level can only be 100 max.
            int targetLevel = Level + levels;
            if (targetLevel > POKEMON_MAX_LEVEL)
                targetLevel = POKEMON_MAX_LEVEL;

            while (Level < targetLevel)
            {
                int currentMaxHP = MaxHP;

                Level += 1;

                // The Pokémon could have a higher Max HP after the level up, so we accommodate for that by raising the current HP:
                int HPDifference = MaxHP - currentMaxHP;
                if (HPDifference > 0)
                    Heal(HPDifference);

                // learn potential level up moves:
                if (learnRandomMove)
                    LearnMove(Level);
            }
        }

        /// <summary>
        /// Attempts to teach this Pokémon a level up move from a specific level. It makes the Pokémon forget a random move if the Pokémon has a full moveset.
        /// </summary>
        /// <returns>Returns true if the Pokémon learned a move.</returns>
        private bool LearnMove(int level)
        {
            var levelMoves = LevelMoves.Where(x => x.Level == level);
            foreach (var levelMove in levelMoves)
            {
                if (levelMove != null)
                {
                    // check if Pokémon does not already know this move:
                    if (!_saveModel.Moves.Any(x => x.Id == levelMove.Id))
                    {
                        var moveList = _saveModel.Moves.ToList();

                        // delete random move when this Pokémon already has 4 moves:
                        if (moveList.Count == POKEMON_MAX_MOVE_COUNT)
                        {
                            moveList.RemoveAt(GlobalRandomProvider.Instance.Rnd.Next(0, moveList.Count));
                        }

                        // get the move model to grab the PP from that:
                        var moveModel = _gameMode.GetMoveModel(levelMove.Id);
                        moveList.Add(new PokemonMoveModel
                        {
                            Id = moveModel.Id,
                            CurrentPP = moveModel.PP,
                            MaxPP = moveModel.PP
                        });

                        _saveModel.Moves = moveList.ToArray();
                    }
                }
            }
            return levelMoves.Count() > 0;
        }

        public void Heal(int healHP)
        {
            HP = MathHelper.Clamp(HP + healHP, 0, MaxHP);
        }
    }
}
