const fs=require('fs'),path=require('path');
const sharp=require('C:/Users/ferre/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root='A:/Projects/Personal/TokTokMart/Assets/GokilKisah';
const dir=path.join(root,'Resources/SmoothArt');fs.mkdirSync(dir,{recursive:true});const sources=path.join(root,'ArtSources');fs.mkdirSync(sources,{recursive:true});
const stroke='#263f46';
const rr=(x,y,w,h,c,r=9)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${r}" fill="${c}" stroke="${stroke}" stroke-width="3"/>`;
const circle=(x,y,r,c)=>`<circle cx="${x}" cy="${y}" r="${r}" fill="${c}"/>`;
const ellipse=(x,y,rx,ry,c)=>`<ellipse cx="${x}" cy="${y}" rx="${rx}" ry="${ry}" fill="${c}"/>`;
async function save(name,body){const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 128 128">${body}</svg>`;fs.writeFileSync(path.join(sources,name+'.svg'),svg);await sharp(Buffer.from(svg)).png().toFile(path.join(dir,name+'.png'));}
(async()=>{
for(const [name,color,skin,hair] of [['Sam','#56bea4','#e5b18c','#343c40'],['Zack','#899ae4','#c78e68','#292e35'],['Customer','#e3b957','#e6b28c','#594235'],['Salmah','#d884a8','#b77d63','#39303d'],['Ray','#7db5c0','#dfb398','#ece5d7'],['Goblin','#7dae6e','#8fc181','#617548'],['Idol','#c4a0db','#a9ced2','#666282']])for(let facing=0;facing<4;facing++)for(let pose=0;pose<3;pose++){
let legs=pose===1?4:pose===2?-4:0;
let body=ellipse(64,116,27,7,'#203c4630')+rr(43,91+legs,17,23,'#344653',7)+rr(68,91-legs,17,23,'#344653',7)+rr(40,54,48,46,color,14)+rr(44,68,40,29,'#f4eddb',4)+rr(44,64,40,12,color,2)+rr(30,61+legs,12,30,skin,6)+rr(86,61-legs,12,30,skin,6)+rr(40,15,48,46,skin,20);
body+=rr(39,12,50,facing===2?48:20,hair,15);
if(facing!==2){let offset=facing===0?-7:facing===1?7:0;body+=circle(54+offset,39,2.6,stroke)+circle(72+offset,39,2.6,stroke)+`<path d="M 56 49 Q 64 54 72 49" fill="none" stroke="${stroke}" stroke-width="2"/>`;}
body+=rr(56,76,16,8,'#ffffff',2);await save(`${name}_${facing}_${pose}`,body);
}
await save('ShelfEmpty',rr(6,14,116,106,'#566d72',8)+rr(10,18,108,32,'#ebddbf',5)+rr(10,53,108,30,'#e2cda7',3)+rr(10,86,108,26,'#d5b889',3));
let products='';for(let row=0;row<2;row++)for(let i=0;i<4;i++)products+=rr(16+i*25,25+row*36,18,27,['#80bfac','#e9ba61','#dc8a85','#93a9d3'][i],3)+rr(19+i*25,33+row*36,12,7,'#f8f1de',1);
await save('ShelfStock',rr(6,14,116,106,'#566d72',8)+rr(10,18,108,91,'#e5d3b2',5)+products+rr(10,105,108,7,'#b6c7b4',1));
await save('Floor','<rect width="128" height="128" fill="#ece9de"/><path d="M 0 0 H128 V128 H0 Z M64 0 V128 M0 64 H128" fill="none" stroke="#dad8ca" stroke-width="1"/>');
await save('Wall',rr(0,0,128,128,'#638f8b',0)+rr(0,86,128,28,'#d9e4d6',0));
await save('Street','<rect width="128" height="128" fill="#64777c"/><path d="M64 12 V44 M64 84 V116" stroke="#e9d9a7" stroke-width="4"/>');
await save('Crate',rr(20,32,88,76,'#ba8761',7)+rr(22,38,84,16,'#e0b284',2)+rr(58,36,12,72,'#ead3a6',2));
await save('Checkout',rr(10,40,108,75,'#517c77',10)+rr(7,30,114,35,'#e0d0b0',9)+rr(45,9,49,35,'#273d47',5)+rr(50,14,39,23,'#8dddc7',3)+rr(25,69,76,23,'#ecdfbb',3)+rr(35,75,56,7,'#889a90',1));
await save('Phone',rr(37,12,54,100,'#e5d7b9',9)+rr(47,40,35,47,'#516e73',5)+rr(31,20,15,67,'#354c55',6)+rr(49,18,31,15,'#8ddbc7',2));
await save('Switch',rr(33,20,62,89,'#efe8d5',9)+rr(44,37,40,48,'#72c2a7',5));
await save('Can',rr(40,23,48,83,'#b5c9c1',13)+ellipse(64,23,24,7,'#e7f0dc')+rr(41,48,46,31,'#e5b764',3));
await save('Spill',ellipse(63,68,50,26,'#77bed28c')+ellipse(38,72,24,21,'#85c8d980')+ellipse(93,66,23,17,'#77bed28c')+'<path d="M31 66 Q46 51 59 55" fill="none" stroke="#e3f8f4" stroke-width="3"/>');
await save('Gun',rr(20,43,91,30,'#7d8e8b',6)+rr(46,68,22,38,'#deac63',4)+rr(91,48,28,17,'#415b63',2));
await save('Lamp',rr(3,42,122,33,'#4c666e',5)+rr(9,47,110,23,'#fff6c7',3));
await save('Car',rr(22,9,84,114,'#ce8175',16)+rr(29,25,70,30,'#aad3d6',7)+rr(29,82,70,20,'#aad3d6',6)+rr(14,30,9,22,'#314550',3)+rr(105,30,9,22,'#314550',3));
console.log('Smooth art exported');
})();
