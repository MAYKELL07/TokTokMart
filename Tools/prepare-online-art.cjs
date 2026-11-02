const fs=require('fs'),path=require('path'),sharp=require('C:/Users/ferre/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const dest='Assets/GokilKisah/Resources/OnlineArt';fs.mkdirSync(dest,{recursive:true});
const characters={Sam:['Male person','malePerson'],Zack:['Male adventurer','maleAdventurer'],Customer:['Female adventurer','femaleAdventurer'],Salmah:['Female person','femalePerson'],Ray:['Male person','malePerson'],Goblin:['Zombie','zombie'],Idol:['Robot','robot']};
(async()=>{const manifest=[];for(const [skin,[folder,id]] of Object.entries(characters))for(let direction=0;direction<4;direction++)for(let pose=0;pose<3;pose++){
 const action=direction===2?'back':direction===3?(pose===0?'idle':'walk'+pose):(pose===0?'side':'walk'+pose);
 const source=path.join('Downloads/toon-characters',folder,'PNG/Poses HD','character_'+id+'_'+action+'.png');
 let bitmap=sharp(source);if(direction===0)bitmap=bitmap.flop();
 await bitmap.resize(192,256,{fit:'contain',background:{r:0,g:0,b:0,alpha:0}}).extend({left:32,right:32,top:0,bottom:0,background:{r:0,g:0,b:0,alpha:0}}).png().toFile(path.join(dest,skin+'_'+direction+'_'+pose+'.png'));
 const original='Assets/GokilKisah/ThirdParty/KenneyToonCharacters/'+path.basename(source);if(!fs.existsSync(original))fs.copyFileSync(source,original);
 manifest.push({sprite:skin+'_'+direction+'_'+pose,source,adaptation:direction===0?'mirrored and padded':'padded to a consistent canvas'});
}fs.writeFileSync('Assets/GokilKisah/ThirdParty/KenneyToonCharacters/sprite-sources.json',JSON.stringify(manifest,null,2));console.log('Downloaded character poses prepared');})();
