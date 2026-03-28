using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace Joi.JoiCode.Minions;

public class ZhouXin : MonsterModel
{
    public override int MinInitialHp => 1;
    public override int MaxInitialHp => 1;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        return null!;
    }
}
