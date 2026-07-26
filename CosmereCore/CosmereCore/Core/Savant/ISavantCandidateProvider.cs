using System;
using Verse;

namespace Cosmere.Core.Savant;

public interface ISavantCandidateProvider {
    void CollectCandidates(Pawn pawn, ICollection<Action> candidates);
}
