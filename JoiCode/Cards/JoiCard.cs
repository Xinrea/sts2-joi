using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public abstract class JoiCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [..GetJoiTermHoverTips()];

    private IEnumerable<IHoverTip> GetJoiTermHoverTips() => Id.Entry.RemovePrefix() switch
    {
        "BLACK_HOLE_STRIKE" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "COMPRESS" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "TIME_ACCELERATION" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "BIRTH" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-AXISCORE"), CreateStaticHoverTip("JOI-SUMMON")],
        "OMNISCIENT" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "HEAVY_HAMMER" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "COSMIC_EXPLOSION" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "TWO_POLE_REVERSAL" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "GRAVITATIONAL_WAVE" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "SPACETIME_DISTORTION" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "SINGULARITY" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "PHOTON_JET" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "BINARY_STAR_COLLISION" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "HAWKING_RADIATION" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "QUANTUM_ENTANGLEMENT" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "DARK_MATTER_IMPACT" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "EVENT_HORIZON" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "SUPERNOVA" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "TIME_DILATION" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "GRAVITATIONAL_LENS" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "GRAVITY_FIELD" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "COSMIC_RIP" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "DIMENSION_JUMP" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "LIGHT_SPEED_ESCAPE" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "PULSE_RAY" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "HEAD_CIRCUMFERENCE_MEASUREMENT" => [CreateStaticHoverTip("JOI-BLACKHOLE")],
        "SNEEZE" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "VACUUM_FLUCTUATION" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "MUA" => [CreateStaticHoverTip("JOI-SUMMON"), CreateStaticHoverTip("JOI-FEED")],
        "WHITE_HOLE_JET" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "WHITE_HOLE_REFLUX" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "PURE_WHITE_FORM" => [CreateStaticHoverTip("JOI-BLACKHOLE"), CreateStaticHoverTip("JOI-WHITEHOLE")],
        "IDOL_CHARM" => [CreateStaticHoverTip("JOI-CHARM")],
        "ORIGINAL_CHICKEN" => [CreateStaticHoverTip("JOI-WHITEHOLE")],
        "SLEEPY_RADIO" => [CreateStaticHoverTip("JOI-SLEEP")],
        "NEBUL_LULLABY" => [CreateStaticHoverTip("JOI-SLEEP")],
        "NIGHTMARE_EROSION" => [CreateStaticHoverTip("JOI-SLEEP"), CreateStaticHoverTip("JOI-BLACKHOLE")],
        "DEEP_SPACE_LULLABY" => [CreateStaticHoverTip("JOI-SLEEP")],
        "RESONANCE_BOND" => [CreateStaticHoverTip("JOI-CHARM"), CreateStaticHoverTip("JOI-WHITEHOLE"), CreateStaticHoverTip("JOI-BLACKHOLE")],
        "GRAVITY_TETHER" => [CreateStaticHoverTip("JOI-CHARM")],
        "CONFUSION_RAY" => [CreateStaticHoverTip("JOI-CHARM")],
        "FEED_ZHOU_XIN" => [CreateStaticHoverTip("JOI-AXISCORE")],
        "ORANGE_PILE" => [CreateStaticHoverTip("JOI-SUMMON")],
        "ORANGE" => [CreateStaticHoverTip("JOI-AXISCORE")],
        "AXIS_CORE_HANDSOME" => [CreateStaticHoverTip("JOI-FEED")],
        "SMELLS_GOOD" => [CreateStaticHoverTip("JOI-AXISCORE")],
        "SORRY_BUDDY" => [CreateStaticHoverTip("JOI-AXISCORE")],
        "MUTATION" => [CreateStaticHoverTip("JOI-AXISCORE")],
        "ORANGE_EXPERT" => [CreateStaticHoverTip("JOI-ORANGE")],
        "ORANGE_TREE" => [CreateStaticHoverTip("JOI-SUMMON"), CreateStaticHoverTip("JOI-ORANGE")],
        _ => []
    };

    private static IHoverTip CreateStaticHoverTip(string locKey) =>
        new HoverTip(
            new LocString("static_hover_tips", $"{locKey}.title"),
            new LocString("static_hover_tips", $"{locKey}.description"),
            null!);

}