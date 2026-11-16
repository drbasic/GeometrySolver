"""Browser smoke test. Start HTTP server on :8000 and geckodriver on :4444 first."""
import urllib.request,json
base='http://127.0.0.1:4444'
def req(path,data=None,method=None):
 r=urllib.request.Request(base+path,data=json.dumps(data).encode() if data is not None else None,headers={'Content-Type':'application/json'},method=method)
 return json.load(urllib.request.urlopen(r))['value']
sid=req('/session',{'capabilities':{'alwaysMatch':{'browserName':'firefox','moz:firefoxOptions':{'args':['-headless']}}}})['sessionId']
def script(s):return req('/session/'+sid+'/execute/sync',{'script':s,'args':[]})

try:
 req('/session/'+sid+'/url',{'url':'http://localhost:8000'})
 result=req('/session/'+sid+'/execute/async',{'script':"""
 const done=arguments[arguments.length-1];import('/geometry.js').then(({intersections:I})=>{
 const line=(x,y,X,Y,type='line')=>({type,a:{x,y},b:{x:X,y:Y}});
 const circle=(x,y,r)=>line(x,y,x+r,y,'circle');
 const checks=[
 I(line(0,0,10,10),line(0,10,10,0)).length===1,
 I(line(0,0,1,0,'segment'),line(2,-1,2,1)).length===0,
 I(line(0,0,10,0),line(0,1,10,1)).length===0,
 I(line(-10,0,10,0),circle(0,0,5)).length===2,
 I(line(-10,5,10,5),circle(0,0,5)).length===1,
 I(line(-10,6,10,6),circle(0,0,5)).length===0,
 I(circle(0,0,5),circle(6,0,5)).length===2,
 I(circle(0,0,5),circle(10,0,5)).length===1,
 I(circle(0,0,5),circle(0,0,5)).length===0,
 I(circle(0,0,5),circle(20,0,5)).length===0
 ];done(checks);}).catch(e=>done(String(e)));
 """,'args':[]})
 assert all(result),result
 script("localStorage.setItem('geometry-editor-v1',JSON.stringify({points:[{id:'p0',name:'A',x:100,y:100},{id:'p1',name:'B',x:300,y:300},{id:'p2',name:'C',x:100,y:300},{id:'p3',name:'D',x:300,y:100}],shapes:[{id:'s4',type:'line',a:'p0',b:'p1'},{id:'s5',type:'line',a:'p2',b:'p3'}],next:6,history:[]}))")
 req('/session/'+sid+'/refresh',{})
 assert script("return document.querySelectorAll('#scene .point').length===5")
 assert script("const p=[...document.querySelectorAll('#scene .point')].pop();return p.getAttribute('cx')==='200' && p.getAttribute('cy')==='200'")
 script("document.querySelector('[data-tool=line]').click();const s=document.querySelector('#canvas'),r=s.getBoundingClientRect();window.tap=(x,y)=>s.dispatchEvent(new PointerEvent('pointerdown',{bubbles:true,clientX:r.left+x,clientY:r.top+y,button:0}));tap(200,200);tap(400,200)")
 assert script("return document.querySelectorAll('#scene .geometry').length===3")
 script("document.querySelector('[data-tool=select]').click();tap(100,100);const s=document.querySelector('#canvas'),r=s.getBoundingClientRect();s.dispatchEvent(new PointerEvent('pointermove',{bubbles:true,clientX:r.left+100,clientY:r.top+200}));s.dispatchEvent(new PointerEvent('pointerup',{bubbles:true}));")
 assert script("return [...document.querySelectorAll('#scene .point')].some(p=>Math.abs(+p.getAttribute('cx')-166.6667)<.01)")
 script("document.querySelectorAll('#objects button')[0].click();document.dispatchEvent(new KeyboardEvent('keydown',{key:'Delete',bubbles:true}))")
 assert script("return document.querySelectorAll('#scene .geometry').length===1")
 script("document.querySelector('#undo').click()")
 assert script("return document.querySelectorAll('#scene .geometry').length===3")
 script("window.beforePan={coords:[...document.querySelectorAll('#scene .point')].map(p=>[p.getAttribute('cx'),p.getAttribute('cy')]),history:document.querySelector('#history').textContent}; const s=document.querySelector('#canvas'),r=s.getBoundingClientRect();s.dispatchEvent(new MouseEvent('mousedown',{button:1,buttons:4,clientX:r.left+100,clientY:r.top+100}));s.dispatchEvent(new PointerEvent('pointercancel'));window.dispatchEvent(new MouseEvent('mousemove',{buttons:4,clientX:r.left+160,clientY:r.top+130}));window.dispatchEvent(new MouseEvent('mouseup',{button:1}));")
 assert script("const s=document.querySelector('#canvas'),v=s.viewBox.baseVal;return v.x===-60 && v.y===-30 && JSON.stringify(beforePan.coords)===JSON.stringify([...document.querySelectorAll('#scene .point')].map(p=>[p.getAttribute('cx'),p.getAttribute('cy')])) && beforePan.history===document.querySelector('#history').textContent")
 script("const s=document.querySelector('#canvas'),r=s.getBoundingClientRect();window.anchorBefore={x:s.viewBox.baseVal.x+200*s.viewBox.baseVal.width/s.clientWidth,y:s.viewBox.baseVal.y+150*s.viewBox.baseVal.height/s.clientHeight};s.dispatchEvent(new WheelEvent('wheel',{clientX:r.left+200,clientY:r.top+150,deltaY:-300,cancelable:true}));")
 assert script("const s=document.querySelector('#canvas'),v=s.viewBox.baseVal;return v.width<s.clientWidth && Math.abs(v.x+200*v.width/s.clientWidth-anchorBefore.x)<.001 && Math.abs(v.y+150*v.height/s.clientHeight-anchorBefore.y)<.001")
 script("document.querySelector('[data-tool=point]').click();tap(450,250)")
 assert script("const s=document.querySelector('#canvas'),v=s.viewBox.baseVal,p=[...document.querySelectorAll('#scene .point')].pop();return Math.abs(+p.getAttribute('cx')-(v.x+450*v.width/s.clientWidth))<.001")
 before=script("const v=document.querySelector('#canvas').viewBox.baseVal;return {x:v.x,y:v.y,width:v.width,screenWidth:document.querySelector('#canvas').clientWidth}")
 rect=script("const r=document.querySelector('#canvas').getBoundingClientRect();return {x:Math.round(r.left+100),y:Math.round(r.top+100)}")
 req('/session/'+sid+'/actions',{'actions':[{'type':'pointer','id':'mouse','parameters':{'pointerType':'mouse'},'actions':[{'type':'pointerMove','origin':'viewport','x':rect['x'],'y':rect['y']},{'type':'pointerDown','button':1},{'type':'pointerMove','origin':'viewport','x':rect['x']+60,'y':rect['y']+30,'duration':250},{'type':'pointerUp','button':1}]}]})
 after=script("const v=document.querySelector('#canvas').viewBox.baseVal;return {x:v.x,y:v.y}")
 assert abs(after['x']-(before['x']-60*before['width']/before['screenWidth']))<.01,(before,after)
 assert abs(after['y']-(before['y']-30*before['width']/before['screenWidth']))<.01,(before,after)
 print('PASS: genuine middle-button browser drag')
 print('PASS: pan preserves coordinates/history; zoom anchor; point placement after transform')
 print('PASS: 10 geometry cases; automatic point; construction from intersection; dependent movement; cascading deletion; undo')
finally:req('/session/'+sid,method='DELETE')
