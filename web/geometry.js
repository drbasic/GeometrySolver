// Pure geometry: segment bounds are respected; coincident curves have no isolated intersections.
const EPS = 1e-8;
const cross = (a, b) => a.x * b.y - a.y * b.x;
const sub = (a, b) => ({ x: a.x - b.x, y: a.y - b.y });
const at = (a, d, t) => ({ x: a.x + d.x * t, y: a.y + d.y * t });
const within = (s, t) => s.type !== 'segment' || (t >= -EPS && t <= 1 + EPS);
export function intersections(s, t) {
 if (s.type === 'circle' && t.type !== 'circle') return intersections(t, s);
 const a = s.a, b = s.b, c = t.a, d = t.b;
 if (s.type !== 'circle' && t.type !== 'circle') {
  const u = sub(b, a), v = sub(d, c), den = cross(u, v);
  if (Math.abs(den) <= EPS * Math.max(1, Math.hypot(u.x,u.y)*Math.hypot(v.x,v.y))) return [];
  const ac = sub(c, a), x = cross(ac, v)/den, y = cross(ac, u)/den;
  return within(s,x) && within(t,y) ? [at(a,u,x)] : [];
 }
 if (s.type !== 'circle') {
  const u = sub(b,a), f = sub(a,c), r = Math.hypot(d.x-c.x,d.y-c.y);
  const A = u.x*u.x+u.y*u.y; if(A <= EPS*EPS || r <= EPS) return [];
  const B = 2*(f.x*u.x+f.y*u.y), C = f.x*f.x+f.y*f.y-r*r;
  const disc = B*B-4*A*C, tol = EPS*Math.max(1,B*B,Math.abs(4*A*C));
  if(disc < -tol) return [];
  const root = Math.sqrt(Math.max(0,disc));
  const roots = root <= Math.sqrt(tol) ? [-B/(2*A)] : [(-B-root)/(2*A),(-B+root)/(2*A)];
  return roots.map((x,branch)=>({...at(a,u,x),branch})).filter((p,i)=>within(s,roots[i]));
 }
 const r = Math.hypot(b.x-a.x,b.y-a.y), R = Math.hypot(d.x-c.x,d.y-c.y), u = sub(c,a), dist = Math.hypot(u.x,u.y);
 if(dist<=EPS || r<=EPS || R<=EPS || dist>r+R+EPS || dist<Math.abs(r-R)-EPS) return [];
 const x=(r*r-R*R+dist*dist)/(2*dist), h2=r*r-x*x;
 if(h2 < -EPS*Math.max(1,r*r)) return [];
 const center=at(a,u,x/dist), h=Math.sqrt(Math.max(0,h2));
 const one={x:center.x-u.y*h/dist,y:center.y+u.x*h/dist};
 return h<=EPS ? [one] : [one,{x:center.x+u.y*h/dist,y:center.y-u.x*h/dist}];
}
