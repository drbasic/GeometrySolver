import { intersections } from './geometry.js';
const $ = s => document.querySelector(s);
const svg = $('#canvas'), scene = $('#scene'), preview = $('#preview');
const NS = 'http://www.w3.org/2000/svg';
let state = { points: [], shapes: [], history: [], next: 0 }, past = [], future = [];
let camera = {x:0,y:0,zoom:1}, pan = null;
let tool = 'point', first = null, selected = null, drag = null, cursor = null;
const clone = x => JSON.parse(JSON.stringify(x));
const point = id => state.points.find(p => p.id === id);
const name = n => String.fromCharCode(65 + n % 26) + (n >= 26 ? Math.floor(n / 26) : '');
const tools = {select:'Выберите объект или перетащите точку',point:'Нажмите на плоскость, чтобы поставить точку',segment:'Укажите начало отрезка',line:'Укажите первую точку прямой',circle:'Укажите центр окружности'};
function element(tag, attrs, parent) { const el = document.createElementNS(NS, tag); for (const [k,v] of Object.entries(attrs)) el.setAttribute(k,v); parent.append(el); return el; }
function save() { past.push(clone(state)); future = []; }
function persist() { recalculate(); try { localStorage.setItem('geometry-editor-v1', JSON.stringify(state)); } catch {} }
function position(e) { const r = svg.getBoundingClientRect(); return {x:camera.x+(e.clientX-r.left)/camera.zoom,y:camera.y+(e.clientY-r.top)/camera.zoom}; }
function nearest(p) { return state.points.filter(p=>p.active!==false).reduce((best,q) => Math.hypot(p.x-q.x,p.y-q.y)<12/camera.zoom && (!best || Math.hypot(p.x-q.x,p.y-q.y)<Math.hypot(p.x-best.x,p.y-best.y)) ? q : best, null); }
function addPoint(p) { const q={id:`p${state.next}`,name:name(state.next++),x:p.x,y:p.y}; state.points.push(q); state.history.push(`Создана точка ${q.name}`); return q; }
// Shape order is dependency order: intersections only depend on earlier shapes.
function recalculate() {
 for (const p of state.points) p.active = !p.parents;
 const ready = [];
 for (const s of state.shapes) {
  const a=point(s.a), b=point(s.b);
  s.active=!!(a?.active && b?.active && Math.hypot(a.x-b.x,a.y-b.y)>1e-8);
  if(!s.active) continue;
  const curve={type:s.type,a,b};
  for(const other of ready) {
   const values=intersections({type:other.type,a:point(other.a),b:point(other.b)},curve);
   values.forEach((value,index)=>{
    const branch=value.branch ?? index;
    let p=state.points.find(p=>p.parents?.[0]===other.id && p.parents?.[1]===s.id && p.branch===branch);
    if(!p) {
     // Shared endpoints already are usable points; do not duplicate them.
     if(state.points.some(q=>q.active && Math.hypot(q.x-value.x,q.y-value.y)<1e-6)) return;
     p={id:`p${state.next}`,name:name(state.next++),parents:[other.id,s.id],branch};
     state.points.push(p);
     state.history.push(`Точка ${p.name} — пересечение: ${label(other)} и ${label(s)}`);
    }
    Object.assign(p,{x:value.x,y:value.y,active:true});
   });
  }
  ready.push(s);
 }
 if(first && !point(first)?.active) first=null;
}
function geometry(s, parent, cls = 'geometry') {
 const a=point(s.a), b=point(s.b); if(!a || !b || s.active===false) return;
 if(s.type==='circle') return element('circle',{cx:a.x,cy:a.y,r:Math.hypot(a.x-b.x,a.y-b.y),class:cls},parent);
 let x1=a.x,y1=a.y,x2=b.x,y2=b.y;
 if(s.type==='line') { const d=Math.hypot(x2-x1,y2-y1); if(d<1e-8)return; const extent=Math.hypot(svg.clientWidth,svg.clientHeight)/camera.zoom+Math.hypot(a.x-camera.x,a.y-camera.y); const dx=(x2-x1)/d*extent,dy=(y2-y1)/d*extent;x1-=dx;y1-=dy;x2=a.x+dx;y2=a.y+dy; }
 return element('line',{x1,y1,x2,y2,class:cls},parent);
}
const label = s => `${{segment:'Отрезок',line:'Прямая',circle:'Окружность'}[s.type]} ${point(s.a).name}${point(s.b).name}`;
function render() {
 recalculate();
 const visiblePoints=state.points.filter(p=>p.active), visibleShapes=state.shapes.filter(s=>s.active);
 const width=svg.clientWidth/camera.zoom, height=svg.clientHeight/camera.zoom;
 svg.setAttribute('viewBox',`${camera.x} ${camera.y} ${width} ${height}`);
 const background=svg.querySelector('rect');
 for(const [key,value] of Object.entries({x:camera.x,y:camera.y,width,height}))background.setAttribute(key,value);
 scene.replaceChildren(); preview.replaceChildren();
 for(const s of visibleShapes) { geometry(s,scene,`geometry${selected===s.id?' selected':''}`); const hit=geometry(s,scene,'hit');if(hit){hit.dataset.id=s.id;hit.style.strokeWidth=16/camera.zoom;} }
 for(const p of visiblePoints) { const el=element('circle',{cx:p.x,cy:p.y,r:5/camera.zoom,class:`point${selected===p.id || first===p.id?' selected':''}`},scene);el.dataset.id=p.id;const t=element('text',{x:p.x+11/camera.zoom,y:p.y-11/camera.zoom,class:'point-label'},scene);t.style.fontSize=`${13/camera.zoom}px`;t.textContent=p.name; }
 if(first && cursor) { const a=point(first), b=nearest(cursor) || cursor; if(a){if(tool==='circle')element('circle',{cx:a.x,cy:a.y,r:Math.hypot(a.x-b.x,a.y-b.y),class:'preview'},preview);else element('line',{x1:a.x,y1:a.y,x2:b.x,y2:b.y,class:'preview'},preview);} }
 $('#objects').replaceChildren();
 for(const o of [...visiblePoints,...visibleShapes]) {const b=document.createElement('button');b.className=`object${selected===o.id?' selected':''}`;const icon=document.createElement('span');icon.className='object-icon';icon.textContent=o.type?{line:'↗',segment:'╱',circle:'○'}[o.type]:'•';const text=document.createElement('span');text.textContent=o.type?label(o):`Точка ${o.name}`;if(!o.type){const small=document.createElement('small');small.textContent=o.parents ? 'Пересечение · зависимая точка' : `${Math.round(o.x)}, ${Math.round(o.y)}`;text.append(small);}b.append(icon,text);b.onclick=()=>{selected=o.id;render();};$('#objects').append(b);}
 if(!state.points.length) {const p=document.createElement('div');p.className='empty';p.textContent='Здесь появятся ваши точки и фигуры';$('#objects').append(p);}
 $('#history').replaceChildren();for(const h of state.history){const li=document.createElement('li');li.textContent=h;$('#history').append(li);}
 $('#object-count').textContent=visiblePoints.length+visibleShapes.length;
 $('#counts').textContent=`Точки: ${visiblePoints.length} · Фигуры: ${visibleShapes.length}`;
 $('#hint').textContent=first ? (tool==='circle'?'Укажите точку на окружности · Esc — отмена':'Укажите вторую точку · Esc — отмена') : tools[tool];
 $('#undo').disabled=!past.length;$('#redo').disabled=!future.length;
 document.querySelectorAll('[data-tool]').forEach(b=>b.classList.toggle('active',b.dataset.tool===tool));
 svg.style.cursor=tool==='select'?'default':'crosshair';
}
function setTool(t){tool=t;first=null;render();}
svg.addEventListener('pointerdown',e=>{
 if(e.button===1)return; // Mouse panning uses independent mouse events below.
 if(e.button!==0 || pan)return;svg.focus();const pos=position(e), near=nearest(pos), id=e.target.dataset.id;
 if(tool==='select'){selected=near?.id || id || null;if(near && !near.parents){drag={id:near.id,before:clone(state),start:pos,moved:false};svg.setPointerCapture(e.pointerId);}render();return;}
 if(tool==='point'){if(near){selected=near.id;render();return;}save();selected=addPoint(pos).id;persist();render();return;}
 if(!first){if(!near)save();const p=near || addPoint(pos);first=p.id;selected=p.id;persist();render();return;}
 const a=point(first);if(Math.hypot(pos.x-a.x,pos.y-a.y)<12/camera.zoom || near?.id===first)return;
 const existing=near && state.shapes.find(s=>s.type===tool && ((s.a===first && s.b===near.id) || (tool!=='circle' && s.b===first && s.a===near.id)));
 if(existing){selected=existing.id;first=null;render();return;}
 save();const b=near || addPoint(pos);const shape={id:`s${state.next++}`,type:tool,a:first,b:b.id};state.shapes.push(shape);state.history.push(`Построена ${label(shape).replace(/^Отрезок/,'отрезок').replace(/^Прямая/,'прямая').replace(/^Окружность/,'окружность')}`);selected=shape.id;first=null;persist();render();
});
svg.addEventListener('pointermove',e=>{
 if(pan)return;
 cursor=position(e);if(drag){const p=point(drag.id);if(!drag.moved && Math.hypot(cursor.x-drag.start.x,cursor.y-drag.start.y)>2/camera.zoom){past.push(drag.before);future=[];drag.moved=true;}if(drag.moved){p.x=cursor.x;p.y=cursor.y;}}if(first || drag)render();});
function endDrag(){if(pan)return;if(drag?.moved){state.history.push(`Перемещена точка ${point(drag.id).name}`);persist();}drag=null;render();}
svg.addEventListener('pointerup',endDrag);svg.addEventListener('pointercancel',endDrag);
function undo(){if(!past.length)return;future.push(clone(state));state=past.pop();first=null;selected=null;persist();render();}
function redo(){if(!future.length)return;past.push(clone(state));state=future.pop();first=null;selected=null;persist();render();}
function remove(){
 if(!selected)return;
 const p=point(selected), s=state.shapes.find(s=>s.id===selected);
 if(p?.parents){$('#hint').textContent='Точка пересечения зависит от фигур. Удалите одну из исходных фигур.';return;}
 const title=p?`точка ${p.name}`:label(s);
 save();const removed=new Set([selected]);let changed=true;
 while(changed){changed=false;for(const o of [...state.points,...state.shapes]){
  if(!removed.has(o.id) && (o.parents?.some(id=>removed.has(id)) || removed.has(o.a) || removed.has(o.b))){removed.add(o.id);changed=true;}
 }}
 state.points=state.points.filter(p=>!removed.has(p.id));state.shapes=state.shapes.filter(s=>!removed.has(s.id));
 state.history.push(`Удалён объект: ${title}; удалены зависимые построения`);selected=null;first=null;persist();render();
}
$('#undo').onclick=undo;$('#redo').onclick=redo;
$('#clear').onclick=()=>{if(!state.points.length)return;save();state={points:[],shapes:[],history:[],next:0};first=null;selected=null;persist();render();};
$('#reset-view').onclick=()=>{
 const points=state.points.filter(p=>p.active);
 if(!points.length){camera={x:0,y:0,zoom:1};render();return;}
 const xs=points.map(p=>p.x),ys=points.map(p=>p.y);
 // Include full circles in the fitted bounds.
 for(const s of state.shapes.filter(s=>s.active && s.type==='circle')){
  const a=point(s.a),b=point(s.b),r=Math.hypot(a.x-b.x,a.y-b.y);
  xs.push(a.x-r,a.x+r);ys.push(a.y-r,a.y+r);
 }
 const minX=Math.min(...xs),maxX=Math.max(...xs),minY=Math.min(...ys),maxY=Math.max(...ys);
 camera.zoom=Math.max(.1,Math.min(8,(svg.clientWidth-80)/Math.max(1,maxX-minX),(svg.clientHeight-80)/Math.max(1,maxY-minY),1));
 camera.x=(minX+maxX)/2-svg.clientWidth/(2*camera.zoom);
 camera.y=(minY+maxY)/2-svg.clientHeight/(2*camera.zoom);render();
};
// Keep middle-button panning independent of pointer capture/cancellation.
svg.addEventListener('mousedown',e=>{
 if(e.button!==1 || drag)return;
 e.preventDefault();e.stopPropagation();
 pan={clientX:e.clientX,clientY:e.clientY,x:camera.x,y:camera.y};
 svg.style.cursor='grabbing';
});
window.addEventListener('mousemove',e=>{
 if(!pan)return;
 if(!(e.buttons & 4)){endPan();return;}
 e.preventDefault();
 camera.x=pan.x-(e.clientX-pan.clientX)/camera.zoom;
 camera.y=pan.y-(e.clientY-pan.clientY)/camera.zoom;
 render();svg.style.cursor='grabbing';
});
function endPan(){if(!pan)return;pan=null;render();}
window.addEventListener('mouseup',e=>{if(e.button===1)endPan();});
window.addEventListener('blur',endPan);
svg.addEventListener('auxclick',e=>{if(e.button===1)e.preventDefault();});
svg.addEventListener('wheel',e=>{
 e.preventDefault();if(pan || drag)return;
 const anchor=position(e), rect=svg.getBoundingClientRect();
 const delta=e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?svg.clientHeight:1);
 camera.zoom=Math.max(.1,Math.min(8,camera.zoom*Math.exp(-delta*.0015)));
 camera.x=anchor.x-(e.clientX-rect.left)/camera.zoom;
 camera.y=anchor.y-(e.clientY-rect.top)/camera.zoom;
 cursor=anchor;render();
},{passive:false});
 document.querySelectorAll('[data-tool]').forEach(b=>b.onclick=()=>setTool(b.dataset.tool));
 document.addEventListener('keydown',e=>{if(e.key==='Escape'){first=null;selected=null;render();}else if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='z'){e.preventDefault();e.shiftKey?redo():undo();}else if(e.key==='Delete' || e.key==='Backspace'){if(selected){e.preventDefault();remove();}}else if(!e.ctrlKey&&!e.metaKey){const t={v:'select',p:'point',s:'segment',l:'line',c:'circle'}[e.key.toLowerCase()];if(t)setTool(t);}});
try{const data=JSON.parse(localStorage.getItem('geometry-editor-v1'));if(data && Array.isArray(data.points) && Array.isArray(data.shapes) && Array.isArray(data.history) && Number.isInteger(data.next))state=data;}catch{}
new ResizeObserver(()=>render()).observe(svg);render();
