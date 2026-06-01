function buildStars(rating) {
  const count = Math.max(0, Math.min(5, Math.round(rating || 0)));
  return '⭐'.repeat(count) || 'Chưa có đánh giá';
}

function excerpt(text) {
  if (!text) return 'Bài viết đang được cập nhật từ database.';
  const clean = text.replace(/\s+/g, ' ').trim();
  return clean.length > 120 ? `${clean.slice(0, 120)}...` : clean;
}

function escapeHtml(text) {
  return String(text ?? '').replace(/[&<>"']/g, char => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  }[char]));
}

function formatDateTime(value) {
  const date = new Date(value || Date.now());
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function categoryIcon(name) {
  const normalized = (name || '').toLowerCase();
  if (normalized.includes('laptop')) return '💻';
  if (normalized.includes('điện thoại')) return '📱';
  if (normalized.includes('phụ kiện')) return '🎧';
  if (normalized.includes('màn hình')) return '🖥️';
  if (normalized.includes('gaming')) return '🎮';
  if (normalized.includes('đồng hồ')) return '⌚';
  return '📝';
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
  const categoriesGrid = document.querySelector('.cats-grid');
  const testimonialsGrid = document.querySelector('.testi-grid');

  try {
    const response = await fetch('/Home/Feed', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    if (!response.ok) throw new Error('feed');

    const data = await response.json();
    const featuredPosts = data.featuredPosts || [];
    const topPosts = data.topPosts || featuredPosts;
    const categories = data.categories || [];
    const comments = data.recentComments || [];
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

    if (categoriesGrid) {
      categoriesGrid.innerHTML = (categories.length ? categories : [{ categoryName: 'Tất cả', postCount: featuredPosts.length }])
        .map(category => {
          const name = category.categoryName || category.CategoryName || 'Danh mục';
          const count = Number(category.postCount ?? category.PostCount ?? 0).toLocaleString('vi-VN');
          return `<a href="#s-products" class="cat-card"><div class="cat-icon">${categoryIcon(name)}</div><div class="cat-name">${escapeHtml(name)}</div><div class="cat-count">${count} bài viết</div></a>`;
        }).join('');
    }

    if (testimonialsGrid) {
      testimonialsGrid.innerHTML = (comments.length ? comments : [{ memberName: 'Chưa có bình luận', content: 'Hãy thêm comment trong database để hiển thị tại đây.', postTitle: 'FTECH', memberAvatarURL: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=120&q=80', createdAt: new Date() }])
        .map(comment => {
          const name = comment.memberName || comment.MemberName || 'Người dùng';
          const content = comment.content || comment.Content || '';
          const title = comment.postTitle || comment.PostTitle || 'Bài viết';
          const avatar = comment.memberAvatarURL || comment.MemberAvatarURL || 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=120&q=80';
          const createdAt = formatDateTime(comment.createdAt || comment.CreatedAt);
          return `<div class="testi-card"><div class="testi-stars">⭐⭐⭐⭐⭐</div><div class="testi-text">"${escapeHtml(excerpt(content))}"</div><div class="testi-author"><div class="testi-avatar"><img src="${avatar}" alt="${escapeHtml(name)}"></div><div><div class="testi-name">${escapeHtml(name)}</div><div class="testi-meta">${escapeHtml(title)} · ${createdAt}</div></div></div><div class="testi-source">Nguồn: database</div></div>`;
        }).join('');
    }
  } catch (error) {
    const flashGrid = document.getElementById('flashGrid');
    const mainGrid = document.getElementById('mainGrid');
    const brandsTrack = document.getElementById('brandsTrack');

    if (flashGrid) flashGrid.innerHTML = '<div class="prod-card"><div class="prod-body"><div class="prod-name">Không tải được dữ liệu từ database.</div></div></div>';
    if (mainGrid) mainGrid.innerHTML = '<div class="prod-card"><div class="prod-body"><div class="prod-name">Vui lòng kiểm tra kết nối SQL Server hoặc dữ liệu bài viết.</div></div></div>';
    if (brandsTrack) brandsTrack.innerHTML = '<div class="brand-item">No data</div>';
    if (categoriesGrid) categoriesGrid.innerHTML = '<div class="cat-card"><div class="cat-icon">📝</div><div class="cat-name">Không tải được danh mục</div><div class="cat-count">0 bài viết</div></div>';
    if (testimonialsGrid) testimonialsGrid.innerHTML = '<div class="testi-card"><div class="testi-text">Không tải được bình luận từ database.</div></div>';
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

function goToReviewSearch(term) {
  const keyword = String(term || '').trim();
  if (!keyword) return;
  window.location.href = `/reviewModule.html?q=${encodeURIComponent(keyword)}`;
}

function ensureNewsletterStatus() {
  let status = document.querySelector('.newsletter-status');
  if (status) return status;

  const form = document.querySelector('.newsletter-form');
  if (!form) return null;

  status = document.createElement('div');
  status.className = 'newsletter-status';
  status.setAttribute('aria-live', 'polite');
  form.insertAdjacentElement('afterend', status);
  return status;
}

function setNewsletterStatus(message, isError = false) {
  const status = ensureNewsletterStatus();
  if (!status) return;

  status.textContent = message || '';
  status.classList.toggle('is-error', Boolean(isError));
  status.classList.toggle('is-success', !isError && Boolean(message));
}

async function submitNewsletter() {
  const input = document.querySelector('.newsletter-input');
  const button = document.querySelector('.newsletter-btn');
  if (!input || !button) return;

  const email = String(input.value || '').trim();
  if (!email) {
    setNewsletterStatus('Vui lòng nhập email để đăng ký.', true);
    return;
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    setNewsletterStatus('Email chưa đúng định dạng.', true);
    return;
  }

  const previousText = button.textContent;
  button.disabled = true;
  button.textContent = 'Đang gửi...';
  setNewsletterStatus('');

  try {
    const response = await fetch('/Home/SubscribeNewsletter', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
        'X-Requested-With': 'XMLHttpRequest'
      },
      body: `email=${encodeURIComponent(email)}`
    });

    if (!response.ok) {
      throw new Error('subscribe');
    }

    const result = await response.json();
    setNewsletterStatus(result.message || 'Đăng ký newsletter thành công.', !result.success);

    if (result.success && !result.alreadySubscribed) {
      input.value = '';
    }
  } catch (error) {
    setNewsletterStatus('Không gửi được đăng ký. Vui lòng thử lại sau.', true);
  } finally {
    button.disabled = false;
    button.textContent = previousText;
  }
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

const headerSearchInput = document.querySelector('.header-search input');
const headerSearchButton = document.querySelector('.header-search button');
if (headerSearchInput) {
  headerSearchInput.addEventListener('keydown', e => {
    if (e.key === 'Enter') {
      e.preventDefault();
      goToReviewSearch(headerSearchInput.value);
    }
  });
}
if (headerSearchButton) {
  headerSearchButton.addEventListener('click', () => {
    goToReviewSearch(headerSearchInput ? headerSearchInput.value : '');
  });
}

const finderInput = document.querySelector('.finder-input');
const finderGo = document.querySelector('.finder-go');
if (finderInput) {
  finderInput.addEventListener('keydown', e => {
    if (e.key === 'Enter') {
      e.preventDefault();
      goToReviewSearch(finderInput.value);
    }
  });
}
if (finderGo) {
  finderGo.addEventListener('click', () => {
    goToReviewSearch(finderInput ? finderInput.value : '');
  });
}

const newsletterInput = document.querySelector('.newsletter-input');
const newsletterButton = document.querySelector('.newsletter-btn');
if (newsletterInput) {
  newsletterInput.addEventListener('keydown', e => {
    if (e.key === 'Enter') {
      e.preventDefault();
      submitNewsletter();
    }
  });
}
if (newsletterButton) {
  newsletterButton.addEventListener('click', submitNewsletter);
}
