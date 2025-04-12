using System.Collections.Generic;
using Verse;

namespace CosmereScadrial.DefModExtensions {
    public class MetalLinked : DefModExtension {
        // Previously just a single metalName
        public List<string> metals;

        public MetalLinked() {
            metals = new List<string>();
        }
    }
}