using Cosmere.Core.Comp.Hediff;
using Cosmere.System.Roshar.Gene;

namespace Cosmere.System.Roshar.Comp.Hediff;

/**
 * Unfortunately necessary for how RimWorld/C# loads XML.
 *
 * Usage:
 * <li Class="Cosmere.Core.Comp.Hediff.SeverityCalculatorProperties">
 *     <compClass>Cosmere.System.Roshar.Comp.Hediff.SeverityCalculator</compClass>
 * </li>
 */
public class SeverityCalculator : SeverityCalculator<Surgebinder>;
