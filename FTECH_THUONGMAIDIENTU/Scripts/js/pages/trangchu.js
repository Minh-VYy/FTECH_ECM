function buildStars(rating) {
  const count = Math.max(0, Math.min(5, Math.round(rating || 0)));
  return '⭐'.repeat(count) || 'Chưa có đánh giá';
}

function excerpt(text) {
  if (!text) return 'Bài viết đang được cập nhật từ database.';
  const clean = text.replace(/\s+/g, ' ').trim();
  return clean.length > 120 ? `${clean.slice(0, 120)}...` : clean;
}

function makeCard(post, compact = false) {
  const title = post.title || post.Title || 'Bài review';
  const category = post.categoryName || post.CategoryName || 'Review';
  const image = post.thumbnailURL || post.ThumbnailURL || 'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&q=80';
  const url = post.url || post.Url || '/Product/Index';
  const views = Number(post.viewCount ?? post.ViewCount ?? 0).toLocaleString('vi-VN');
  const rating = Number(post.averageRating ?? post.AverageRating ?? 0);
  const reviewCount = Number(post.ratingCount ?? post.RatingCount ?? 0);

  return `<div class="prod-card">
    <div class="prod-img"><div class="prod-fav">🤍</div><img src="${image}" alt="${title}"></div>
    <div class="prod-body">
      <div class="prod-brand">${category}</div>
      <div class="prod-name">${title}</div>
      <div class="prod-stars"><span class="stars">${buildStars(rating)}</span><span class="reviews">(${views} lượt xem${reviewCount ? ` · ${reviewCount} đánh giá` : ''})</span></div>
      ${compact ? '' : `<div class="prod-desc">${excerpt(post.content || post.Content)}</div>`}
      <a class="prod-add" href="${url}">Xem review chi tiết</a>
    </div>
  </div>`;
}

async function loadHomeFeed() {
  try {
    const response = await fetch('/Home/Feed', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    if (!response.ok) throw new Error('feed');

    const data = await response.json();
    const featuredPosts = data.featuredPosts || [];
    const topPosts = data.topPosts || featuredPosts;
    const partners = data.partnerNames || [];

    const flashGrid = document.getElementById('flashGrid');
    const mainGrid = document.getElementById('mainGrid');
    const brandsTrack = document.getElementById('brandsTrack');

    if (flashGrid) {
      flashGrid.innerHTML = (featuredPosts.slice(0, 5).length ? featuredPosts.slice(0, 5) : topPosts.slice(0, 5)).map(item => makeCard(item, true)).join('');
    }

    if (mainGrid) {
      const mainItems = topPosts.length ? topPosts : featuredPosts;
      mainGrid.innerHTML = mainItems.map(item => makeCard(item)).join('');
    }

    if (brandsTrack) {
      const names = partners.map(p => p.partnerName || p.PartnerName).filter(Boolean);
      brandsTrack.innerHTML = (names.length ? [...names, ...names] : ['Shopee', 'Lazada', 'Tiki']).map(name => `<div class="brand-item">${name}</div>`).join('');
    }
  } catch (error) {
    const flashGrid = document.getElementById('flashGrid');
    const mainGrid = document.getElementById('mainGrid');
    const brandsTrack = document.getElementById('brandsTrack');

    if (flashGrid) flashGrid.innerHTML = '<div class="prod-card"><div class="prod-body"><div class="prod-name">Không tải được dữ liệu từ database.</div></div></div>';
    if (mainGrid) mainGrid.innerHTML = '<div class="prod-card"><div class="prod-body"><div class="prod-name">Vui lòng kiểm tra kết nối SQL Server hoặc dữ liệu bài viết.</div></div></div>';
    if (brandsTrack) brandsTrack.innerHTML = '<div class="brand-item">No data</div>';
  }
}

function makeLabel(t) {
  if (!t) return '';
  const map = {sale:'label-sale',new:'label-new',hot:'label-hot'};
  const txt = {sale:'SALE',new:'MỚI',hot:'HOT'};
  return `<div class="prod-labels"><span class="label ${map[t]}">${txt[t]}</span></div>`;
}
function makeMockCard(p) {
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

loadHomeFeed();

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
