const flashProducts = [
  { image:'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=900&q=80', brand:'Shopee Mall', name:'iPhone 16 Pro Max - giá tham khảo hôm nay', price:'29.990.000₫', old:'31.490.000₫', discount:'-5%', label:'hot', stars:5 },
  { image:'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=80', brand:'Lazada Mall', name:'WH-1000XM5 - đối tác có voucher tốt', price:'6.490.000₫', old:'6.990.000₫', discount:'-7%', label:'new', stars:5 },
  { image:'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&q=80', brand:'FTECH Picks', name:'ROG Zephyrus G14 - deal tốt cho gaming', price:'31.990.000₫', old:'33.500.000₫', discount:'-4%', label:'sale', stars:4 },
  { image:'https://images.unsplash.com/photo-1546868871-7041f2a55e12?auto=format&fit=crop&w=900&q=80', brand:'Tiki Trading', name:'Apple Watch Series 10 - đối tác còn hàng', price:'10.990.000₫', old:'11.590.000₫', discount:'-5%', label:'new', stars:5 },
  { image:'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=900&q=80', brand:'CameraHouse', name:'EOS R50 - ưu đãi combo phụ kiện', price:'18.490.000₫', old:'19.290.000₫', discount:'-4%', label:'sale', stars:5 },
];
const mainProducts = [
  { image:'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=900&q=80', brand:'Apple',   name:'iPhone 16 Pro Max 256GB: có đáng nâng cấp từ iPhone 14?', price:'34.990.000₫', old:'39.900.000₫', discount:'-12%', label:'new', stars:5, reviews:1240 },
  { image:'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&q=80', brand:'Dell',    name:'Dell XPS 15 Core i7-14700H: laptop cho dân sáng tạo?',    price:'42.990.000₫', old:'49.000.000₫', discount:'-12%', label:'sale', stars:4, reviews:380 },
  { image:'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=80', brand:'Apple',   name:'AirPods Pro 2: nên chọn Apple hay Sony ở tầm giá này?',    price:'6.490.000₫', old:null, discount:null, label:'new', stars:5, reviews:920 },
  { image:'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=900&q=80', brand:'LG',     name:'LG UltraGear 27 QHD 165Hz: màn gaming đáng mua?',          price:'8.990.000₫', old:'10.500.000₫', discount:'-14%', label:'sale', stars:4, reviews:210 },
  { image:'https://images.unsplash.com/photo-1593784991095-a205069470b6?auto=format&fit=crop&w=900&q=80', brand:'Samsung', name:'Samsung Neo QLED 4K 55: phù hợp phòng khách nào?',         price:'19.990.000₫', old:'24.900.000₫', discount:'-20%', label:'hot', stars:5, reviews:450 },
  { image:'https://images.unsplash.com/photo-1606144042614-b2417e99c4e3?auto=format&fit=crop&w=900&q=80', brand:'Sony',    name:'PlayStation 5 Slim: có phải thời điểm tốt để mua?',        price:'14.990.000₫', old:null, discount:null, label:'new', stars:5, reviews:870 },
  { image:'https://images.unsplash.com/photo-1546868871-7041f2a55e12?auto=format&fit=crop&w=900&q=80', brand:'Garmin',  name:'Garmin Fenix 7X Solar: dành cho ai, có đáng giá không?',    price:'18.490.000₫', old:'21.000.000₫', discount:'-12%', label:null, stars:4, reviews:160 },
  { image:'https://images.unsplash.com/photo-1545454675-3531b543be5d?auto=format&fit=crop&w=900&q=80', brand:'Sonos',   name:'Sonos Era 300: loa Dolby Atmos cho người thích trải nghiệm', price:'12.990.000₫', old:null, discount:null, label:'new', stars:5, reviews:290 },
];
const brands = ['Apple','Samsung','Sony','Asus','Dell','LG','Xiaomi','Lenovo','JBL','Logitech','Canon','Garmin'];

function makeLabel(t) {
  if (!t) return '';
  const map = {sale:'label-sale',new:'label-new',hot:'label-hot'};
  const txt = {sale:'SALE',new:'MỚI',hot:'HOT'};
  return `<div class="prod-labels"><span class="label ${map[t]}">${txt[t]}</span></div>`;
}
function makeCard(p) {
  return `<div class="prod-card">
    <div class="prod-img">${makeLabel(p.label)}<div class="prod-fav">🤍</div><img src="${p.image}" alt="${p.name}"></div>
    <div class="prod-body">
      <div class="prod-brand">${p.brand}</div>
      <div class="prod-name">${p.name}</div>
      <div class="prod-stars"><span class="stars">${'⭐'.repeat(p.stars)}</span>${p.reviews?`<span class="reviews">(${p.reviews.toLocaleString()})</span>`:''}</div>
      <div class="prod-price-row">
        <span class="prod-price-new">${p.price}</span>
        ${p.old?`<span class="prod-price-old">${p.old}</span>`:''}
        ${p.discount?`<span class="prod-discount">${p.discount}</span>`:''}
      </div>
      <a class="prod-add" href="product.html">Xem review & nơi mua</a>
    </div>
  </div>`;
}

document.getElementById('flashGrid').innerHTML = flashProducts.map(makeCard).join('');
document.getElementById('mainGrid').innerHTML  = mainProducts.map(makeCard).join('');
document.getElementById('brandsTrack').innerHTML = [...brands,...brands].map(b=>`<div class="brand-item">${b}</div>`).join('');

document.querySelectorAll('.tab').forEach(t => t.addEventListener('click', function() {
  document.querySelectorAll('.tab').forEach(x=>x.classList.remove('active'));
  this.classList.add('active');
}));

let secs = 8*3600+24*60+55;
function updateTimer() {
  const h=String(Math.floor(secs/3600)).padStart(2,'0');
  const m=String(Math.floor((secs%3600)/60)).padStart(2,'0');
  const s=String(secs%60).padStart(2,'0');
  const el=document.getElementById('flash-timer');
  if(el) el.textContent=`${h}:${m}:${s}`;
  if(secs>0) secs--;
}
setInterval(updateTimer, 1000); updateTimer();

const obs = new IntersectionObserver(entries => {
  entries.forEach(e => { if(e.isIntersecting){e.target.classList.add('visible');obs.unobserve(e.target);} });
}, {threshold:0.08});
document.querySelectorAll('.fade-up').forEach(el => obs.observe(el));

const si = document.querySelector('.header-search input');
si.addEventListener('keypress', e => { if(e.key==='Enter'&&si.value.trim()) alert('Tìm: '+si.value); });
document.querySelector('.header-search button').addEventListener('click', () => { if(si.value.trim()) alert('Tìm: '+si.value); });
