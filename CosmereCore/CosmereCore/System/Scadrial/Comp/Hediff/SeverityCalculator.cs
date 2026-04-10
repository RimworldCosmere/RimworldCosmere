using Cosmere.Core.Comp.Hediff;
using Cosmere.System.Scadrial.Gene;

namespace Cosmere.System.Scadrial.Comp.Hediff;

/**
 * Unfortunately necessary for how RimWorld/C# loads XML.
 * 
 * Usage:
 * <li Class="Cosmere.Core.Comp.Hediff.SeverityCalculatorProperties">
 *     <compClass>Cosmere.System.Scadrial.Comp.Hediff.SeverityCalculator</compClass>
 * </li>
 */
public class SeverityCalculator : SeverityCalculator<Allomancer>;