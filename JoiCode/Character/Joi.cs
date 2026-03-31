using BaseLib.Abstracts;
using Joi.JoiCode.Cards;
using Joi.JoiCode.Extensions;
using Joi.JoiCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Joi.JoiCode.Character;

public class Joi : PlaceholderCharacterModel
{
	public const string CharacterId = "Joi";

	public override string PlaceholderID => "ironclad";

	public static readonly Color Color = new("ffce65");

	public override Color NameColor => Color;
	public override CharacterGender Gender => CharacterGender.Neutral;
	public override int StartingHp => 70;
	
	public override IEnumerable<CardModel> StartingDeck => [
		ModelDb.Card<Strike>(),
		ModelDb.Card<Strike>(),
		ModelDb.Card<Strike>(),
		ModelDb.Card<Strike>(),
		ModelDb.Card<Strike>(),
		ModelDb.Card<Defend>(),
		ModelDb.Card<Defend>(),
		ModelDb.Card<Defend>(),
		ModelDb.Card<Defend>(),
		ModelDb.Card<BlackHoleStrike>()
	];

	public override IReadOnlyList<RelicModel> StartingRelics =>
	[
		ModelDb.Relic<CosmicOriginRelic>()
	];
	
	public override CardPoolModel CardPool => ModelDb.CardPool<JoiCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<JoiRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<JoiPotionPool>();
	
	/*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
		override all the other methods that define those assets. 
		These are just some of the simplest assets, given some placeholders to differentiate your character with. 
		You don't have to, but you're suggested to rename these images. */
	public override string CustomIconTexturePath => "res://Joi/images/charui/character_icon_char_name.png";
	public override string CustomIconPath => "res://scenes/joi_icon.tscn";
	public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
	public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
	public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
	public override string CustomVisualPath => "res://scenes/joi.tscn";
	public override string CustomCharacterSelectBg => "res://scenes/joi_bg.tscn";
}
