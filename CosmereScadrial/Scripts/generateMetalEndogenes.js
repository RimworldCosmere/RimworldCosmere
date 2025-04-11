const fs = require('fs');
const path = require('path');

const metals = require('..\\..\\CosmereCore\\Data\\metals.json');

function generateGeneDef(metal, type, parent, label, description, displayOrderInCategory) {
  const upperName = metal.name.charAt(0).toUpperCase() + metal.name.slice(1);
  const defName = `${type}_${upperName}`;
  return `  <GeneDef ParentName="Cosmere_${parent}">
    <defName>Cosmere_${defName}</defName>
    <label>${label}</label>
    <labelShortAdj>${type === 'Misting' ? metal.allomancerName : metal.feruchemistName}</labelShortAdj>
    <description>${description}</description>
    <displayOrderInCategory>${displayOrderInCategory}</displayOrderInCategory>
    <iconPath>UI/Icons/Genes/Investiture/${parent === 'AllomanticBase' ? 'Allomancy' : 'Feruchemy'}/${upperName}</iconPath>
    <modExtensions>
      <li Class="CosmereScadrial.DefModExtensions.GeneModExtension_AllomanticMetals">
        <metals>
          <li>${metal.name}</li>
        </metals>
      </li>
    </modExtensions>
  </GeneDef>`;
}

const lines = [
  '<?xml version="1.0" ?>',
  '<Defs>'
];

i = 4;
metals.forEach((metal) => {
  lines.push(`  <!-- ${metal.name} -->`)
  if (metal.allomancerName) {
    lines.push(generateGeneDef(metal, 'Misting', 'AllomanticBase', `${metal.name} Misting`, metal.descriptionAllomancer, i++));
  }
  if (metal.feruchemistName) {
    lines.push(generateGeneDef(metal, 'Ferring', 'FeruchemicBase', `${metal.name} Ferring`, metal.descriptionFeruchemist, i++));
  }
  lines.push('');
});

lines.push('</Defs>');

const fileName = path.join(__dirname, '..\\Defs\\Investiture\\EndoGenes.xml');
fs.writeFileSync(fileName, lines.join('\n'), 'utf8');
console.log(`✅ Generated ${fileName}`);