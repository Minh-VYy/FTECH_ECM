let mainStar = 0;
let photoCount = 0;
let shownCount = 3;
let loadedReviews = [];

const starLabels = ['', 'Rất tệ', 'Không hài lòng', 'Bình thường', 'Hài lòng', 'Tuyệt vời!'];

function setMainStar(v) {
  mainStar = v;
  document.querySelectorAll('.sp-star').forEach((s, i) => { s.classList.toggle('lit', i < v); });
  const label = document.getElementById('starLabel');
  if (label) {
    label.textContent = starLabels[v];
    label.style.color = '#fbbf24';
  }
}

function setMini(group, v) {
  const container = document.querySelector(`.mini-stars[data-group="${group}"]`);
  if (!container) return;
  container.querySelectorAll('.mstar').forEach((s, i) => s.classList.toggle('lit', i < v));
}

const rvText = document.getElementById('rvText');
if (rvText) {
  rvText.addEventListener('input', function () {
    const charCount = document.getElementById('charCount');
    if (charCount) {
      charCount.textContent = this.value.length + ' / 1000 ký tự';
    }
  });
}

function addPhoto() {
  if (photoCount >= 5) {
    alert('Tối đa 5 ảnh.');
    return;
  }

  const container = document.getElementById('uploadedImgs');
  if (!container) return;

  const photoEmojis = ['🖼️', '📸', '🔍', '💡', '🎯'];
  const item = document.createElement('div');
  item.className = 'uimg';
  item.innerHTML = photoEmojis[photoCount] + '<div class="del" onclick="this.parentElement.remove();photoCount--">×</div>';
  container.appendChild(item);
  photoCount++;
}

function submitReview() {
  if (mainStar === 0) {
    alert('Vui lòng chọn số sao đánh giá.');
    return;
  }

  if (!document.getElementById('rvText')?.value.trim()) {
    alert('Vui lòng nhập nội dung đánh giá.');
    return;
  }

  const btn = document.querySelector('.submit-btn');
  if (!btn) return;

  btn.disabled = true;
  btn.textContent = '⏳ Đang gửi...';
  setTimeout(() => {
    document.getElementById('toastSuccess')?.classList.add('show');
    btn.textContent = '✅ Đã gửi!';
    if (rvText) rvText.value = '';
    const rvTitle = document.getElementById('rvTitle');
    if (rvTitle) rvTitle.value = '';
    document.querySelectorAll('.sp-star').forEach(s => s.classList.remove('lit'));
    document.querySelectorAll('.mstar').forEach(s => s.classList.remove('lit'));
    mainStar = 0;
    photoCount = 0;
    const uploadedImgs = document.getElementById('uploadedImgs');
    if (uploadedImgs) uploadedImgs.innerHTML = '';
    const starLabel = document.getElementById('starLabel');
    if (starLabel) starLabel.textContent = 'Nhấn để chọn sao';
    setTimeout(() => {
      btn.disabled = false;
      btn.textContent = '📤 Gửi đánh giá';
      document.getElementById('toastSuccess')?.classList.remove('show');
    }, 3000);
  }, 1000);
}

function buildStars(rating) {
  const count = Math.max(0, Math.min(5, Math.round(rating || 0)));
  return count > 0 ? '⭐'.repeat(count) : 'Chưa có đánh giá';
}

function excerpt(text) {
  if (!text) return 'Bài viết đang được cập nhật từ database.';
  const clean = text.replace(/\s+/g, ' ').trim();
  return clean.length > 160 ? `${clean.slice(0, 160)}...` : clean;
}

function renderReviews(list) {
  const container = document.getElementById('reviewsList');
  if (!container) return;

  const slice = list.slice(0, shownCount);
  container.innerHTML = slice.map(post => {
    const title = post.title || post.Title || 'Bài review';
    const category = post.categoryName || post.CategoryName || 'Review';
    const date = new Date(post.createdAt || post.CreatedAt || Date.now());
    const rating = Number(post.averageRating ?? post.AverageRating ?? 0);
    const viewCount = Number(post.viewCount ?? post.ViewCount ?? 0).toLocaleString('vi-VN');
    const ratingCount = Number(post.ratingCount ?? post.RatingCount ?? 0);
    const url = post.url || post.Url || '/Product/Index';
    const image = post.thumbnailURL || post.ThumbnailURL || 'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=120&q=80';

    return `
    <div class="rv-card">
      <div class="rv-top">
        <div class="rv-user">
          <div class="rv-avatar"><img src="${image}" alt="${title}"></div>
          <div>
            <div class="rv-name">${title}</div>
            <div class="rv-meta">${date.toLocaleDateString('vi-VN')} · ${category}</div>
            <div class="rv-source">${viewCount} lượt xem · ${ratingCount} đánh giá</div>
          </div>
        </div>
        <div class="rv-right">
          <span class="rv-stars-row">${buildStars(rating)}</span>
          <span class="rv-score">${rating > 0 ? rating.toFixed(1) : '0.0'}</span>
        </div>
      </div>
      <div class="rv-title">Nội dung từ database</div>
      <div class="rv-text">${excerpt(post.content || post.Content)}</div>
      <div class="rv-actions">
        <span class="rv-helpful">Đọc chi tiết bài viết</span>
        <a class="rv-helpful-btn" href="${url}">Mở bài review</a>
      </div>
    </div>`;
  }).join('');
}

async function loadReviewFeed() {
  try {
    const response = await fetch('/Review/Feed', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    if (!response.ok) throw new Error('feed');

    const data = await response.json();
    loadedReviews = data.reviews || [];
    renderReviews(loadedReviews);
  } catch (error) {
    const container = document.getElementById('reviewsList');
    if (container) {
      container.innerHTML = '<div class="rv-card"><div class="rv-title">Không tải được dữ liệu từ database.</div><div class="rv-text">Vui lòng kiểm tra SQL Server hoặc dữ liệu bài review đã xuất bản.</div></div>';
    }
  }
}

function vote(btn, id, dir) {
  if (btn.classList.contains('voted')) return;
  btn.classList.add('voted');
}

function filterTab(btn, val) {
  document.querySelectorAll('.ftab').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
}

function filterByStar(n) {
  document.querySelectorAll('.ftab').forEach((b, i) => { b.classList.toggle('active', i === n - 1 || i === 5 - n + 1); });
}

function sortReviews(v) { }

function loadMoreReviews() {
  shownCount = Math.min(shownCount + 5, loadedReviews.length || shownCount + 5);
  if (loadedReviews.length) {
    renderReviews(loadedReviews);
  }
}

loadReviewFeed();
