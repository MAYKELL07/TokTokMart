from pathlib import Path
p=Path('Assets/GokilKisah/Editor/ShiftSceneUpgrade.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('else if(name.StartsWith("Floor ")){art="Floor";if(expanding)size=Vector2.Scale(size,new Vector2(1.65f,1.8f));}', '''else if(name.StartsWith("Floor "))
                {
                    art="Floor";var coordinates=name.Substring(6).Split(',');
                    float x=float.Parse(coordinates[0]),y=float.Parse(coordinates[1]);
                    size=new Vector2(Mathf.Min(48,1355-x)/100*1.65f,Mathf.Min(48,735-y)/100*1.8f);
                }''');s=s.replace('else if(name.Contains("wall")||name.Contains("Wall")){art="Wall";if(expanding)size=Vector2.Scale(size,new Vector2(1.65f,1.8f));}', 'else if(name.Contains("wall")||name.Contains("Wall")){art="Wall";size=name.Contains("Back")?new Vector2(.48f*1.65f,.36f*1.8f):new Vector2(.26f*1.65f,.48f*1.8f);}')
s=s.replace('else if(name=="Street"){art="Street";if(expanding)size=Vector2.Scale(size,new Vector2(1.65f,1.8f));}', '''else if(name=="Street")
                {
                    art="Street";var center=ShopLayout.ToGame(renderer.transform.position);
                    float x=320+(center.x-320)/1.65f,y=187+(center.y-187)/1.8f;
                    size=new Vector2((x>1534?21:48)/100f*1.65f,(y>704?39:48)/100f*1.8f);
                }''');p.write_text(s,encoding='utf-8')
