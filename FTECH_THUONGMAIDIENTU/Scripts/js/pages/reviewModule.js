let mainStar = 0;
let photoCount = 0;
let shownCount = 1000;
let loadedReviews = [];

function getSearchQuery() {
  return new URLSearchParams(window.location.search).get('q') || '';
}

function getQueryParams() {
  const params = new URLSearchParams(window.location.search);
  const pathParts = window.location.pathname.split('/');
  const lastPart = pathParts[pathParts.length - 1];
  const pathId = isNaN(Number(lastPart)) ? '' : lastPart;
  
  return {
    q: params.get('q') || '',
    brand: params.get('brand') || '',
    sort: params.get('sort') || '',
    category: params.get('category') || '',
    id: params.get('id') || pathId
  };
}

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

async function submitReview() {
  const { id } = getQueryParams();
  if (!id) {
    alert('Không xác định được sản phẩm để đánh giá.');
    return;
  }

  if (mainStar === 0) {
    alert('Vui lòng chọn số sao đánh giá.');
    return;
  }

  const textVal = document.getElementById('rvText')?.value.trim();
  if (!textVal) {
    alert('Vui lòng nhập nội dung đánh giá.');
    return;
  }

  const btn = document.querySelector('.submit-btn');
  if (!btn) return;

  btn.disabled = true;
  btn.textContent = '⏳ Đang gửi...';

  try {
    const response = await fetch('/Product/AddComment', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest'
      },
      body: `postId=${id}&content=${encodeURIComponent(textVal)}&ratingStar=${mainStar}`
    });

    if (!response.ok) throw new Error('Không thể kết nối máy chủ.');
    const result = await response.json();

    if (result.success) {
      document.getElementById('toastSuccess')?.classList.add('show');
      btn.textContent = '✅ Đã gửi!';
      setTimeout(() => {
        window.location.reload();
      }, 1500);
    } else {
      alert(result.message || 'Có lỗi xảy ra.');
      btn.disabled = false;
      btn.textContent = '📤 Gửi đánh giá';
    }
  } catch (err) {
    alert(err.message || 'Lỗi gửi đánh giá. Vui lòng kiểm tra lại kết nối.');
    btn.disabled = false;
    btn.textContent = '📤 Gửi đánh giá';
  }
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

    const isRealReview = post.isRealReview || post.IsRealReview;
    
    let editOpsHtml = '';
    if (isRealReview && typeof currentMemberId !== 'undefined' && currentMemberId && post.MemberID === currentMemberId) {
      if (post.EditCount < 1) {
        editOpsHtml = `<div class="comment-ops" style="margin-top: 8px; display: flex; gap: 12px; font-size: 12px;">
          <a href="javascript:void(0)" onclick="editReview(${post.CommentID}, '${escapeJsString(post.Content)}')" style="color: var(--accent); font-weight: 600; text-decoration: none; display: inline-flex; align-items: center; gap: 4px;">✏️ Sửa (còn 1 lần)</a>
          <a href="javascript:void(0)" onclick="deleteReview(${post.CommentID})" style="color: var(--red); font-weight: 600; text-decoration: none; display: inline-flex; align-items: center; gap: 4px; margin-left: 8px;">🗑️ Xóa đánh giá</a>
        </div>`;
      } else {
        editOpsHtml = `<div class="comment-ops" style="margin-top: 8px; display: flex; gap: 12px; font-size: 12px;">
          <span style="color: var(--muted); font-style: italic;">✏️ Đã hết lượt sửa</span>
          <a href="javascript:void(0)" onclick="deleteReview(${post.CommentID})" style="color: var(--red); font-weight: 600; text-decoration: none; display: inline-flex; align-items: center; gap: 4px; margin-left: 8px;">🗑️ Xóa đánh giá</a>
        </div>`;
      }
    }

    const ratingDisplay = rating > 0 ? rating.toFixed(1) : '5.0';
    const starsDisplay = buildStars(rating || 5);

    return `
    <div class="rv-card">
      <div class="rv-top">
        <div class="rv-user">
          <div class="rv-avatar"><img src="${image}" alt="${title}"></div>
          <div>
            <div class="rv-name">${title}</div>
            <div class="rv-meta">${date.toLocaleDateString('vi-VN')} · ${category} ${post.EditCount >= 1 ? '<span style="color: var(--muted); font-size: 11px; font-style: italic; margin-left: 6px;">(Đã sửa)</span>' : ''}</div>
            ${isRealReview ? '' : `<div class="rv-source">${viewCount} lượt xem · ${ratingCount} đánh giá</div>`}
          </div>
        </div>
        <div class="rv-right">
          <span class="rv-stars-row">${starsDisplay}</span>
          <span class="rv-score">${ratingDisplay}</span>
        </div>
      </div>
      <div class="rv-title">${isRealReview ? 'Đánh giá thực tế từ khách mua hàng' : 'Nội dung từ database'}</div>
      <div class="rv-text">${excerpt(post.content || post.Content)}</div>
      <div class="rv-actions">
        ${isRealReview ? '' : `
          <span class="rv-helpful">Đọc chi tiết bài viết</span>
          <a class="rv-helpful-btn" href="${url}">Mở bài review</a>
        `}
      </div>
      ${editOpsHtml}
    </div>`;
  }).join('');
}

async function loadReviewFeed() {
  const { id, q, brand, sort, category } = getQueryParams();
  try {
    const params = [];
    if (id) params.push(`id=${encodeURIComponent(id)}`);
    if (q) params.push(`q=${encodeURIComponent(q)}`);
    if (brand) params.push(`brand=${encodeURIComponent(brand)}`);
    if (sort) params.push(`sort=${encodeURIComponent(sort)}`);
    if (category) params.push(`category=${encodeURIComponent(category)}`);
    
    const queryString = params.length ? `?${params.join('&')}` : '';
    const feedUrl = `/Review/Feed${queryString}`;
    const response = await fetch(feedUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    if (!response.ok) throw new Error('feed');

    const data = await response.json();
    loadedReviews = data.reviews || [];

    const title = document.querySelector('.rh-title');
    if (title) {
      const filters = [q, brand, category].filter(Boolean);
      title.textContent = filters.length ? `Kết quả tìm kiếm: ${filters.join(' - ')}` : (id ? 'Tất cả đánh giá của sản phẩm' : 'Tất cả đánh giá');
    }

    applyFilterAndSort();
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

let currentFilter = 'all';
let currentSort = 'newest';

// Initialize sort from URL parameter to respect search filters selected on homepage
const initQueryParams = getQueryParams();
if (initQueryParams.sort) {
  const s = initQueryParams.sort.trim().toLowerCase();
  if (s.includes('lượt xem') || s === 'helpful' || s === 'views') {
    currentSort = 'helpful';
  } else if (s.includes('đánh giá cao') || s === 'highest' || s === 'rating') {
    currentSort = 'highest';
  } else if (s.includes('so sánh') || s === 'featured') {
    currentSort = 'helpful';
  } else {
    currentSort = 'newest';
  }
}

function filterTab(btn, val) {
  document.querySelectorAll('.ftab').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
  currentFilter = val;
  applyFilterAndSort();
}

function filterByStar(n) {
  const val = String(n);
  const matchingTab = Array.from(document.querySelectorAll('.ftab')).find(b => b.getAttribute('onclick')?.includes(`'${val}'`));
  if (matchingTab) {
    filterTab(matchingTab, val);
  }
}

function sortReviews(v) {
  currentSort = v;
  applyFilterAndSort();
}

function applyFilterAndSort() {
  let list = [...loadedReviews];
  
  // Filter
  if (currentFilter !== 'all') {
    if (['1', '2', '3', '4', '5'].includes(currentFilter)) {
      const star = Number(currentFilter);
      list = list.filter(r => Math.round(Number(r.averageRating ?? r.AverageRating ?? 0)) === star);
    } else if (currentFilter === 'photo') {
      list = list.filter(r => !!(r.thumbnailURL || r.ThumbnailURL));
    } else if (currentFilter === 'verified') {
      // Keep all or filter based on rating count
    }
  }
  
  // Sort
  if (currentSort === 'newest') {
    list.sort((a, b) => new Date(b.createdAt || b.CreatedAt) - new Date(a.createdAt || a.CreatedAt));
  } else if (currentSort === 'highest') {
    list.sort((a, b) => Number(b.averageRating ?? b.AverageRating ?? 0) - Number(a.averageRating ?? a.AverageRating ?? 0));
  } else if (currentSort === 'lowest') {
    list.sort((a, b) => Number(a.averageRating ?? a.AverageRating ?? 0) - Number(b.averageRating ?? b.AverageRating ?? 0));
  } else if (currentSort === 'helpful') {
    list.sort((a, b) => Number(b.viewCount ?? b.ViewCount ?? 0) - Number(a.viewCount ?? a.ViewCount ?? 0));
  } else if (currentSort === 'photos') {
    list = list.filter(r => !!(r.thumbnailURL || r.ThumbnailURL));
  }
  
  renderReviews(list);
}

function loadMoreReviews() {
  shownCount = Math.min(shownCount + 5, loadedReviews.length || shownCount + 5);
  if (loadedReviews.length) {
    renderReviews(loadedReviews);
  }
}

async function editReview(commentId, oldContent) {
  const newContent = prompt("Chỉnh sửa nội dung đánh giá của bạn (Chỉ được sửa tối đa 1 lần):", oldContent);
  if (newContent === null) return; // User cancelled
  const trimmed = newContent.trim();
  if (!trimmed) {
    alert("Nội dung không được để trống.");
    return;
  }

  const newStarStr = prompt("Chỉnh sửa số sao đánh giá mới (1 đến 5 sao) hoặc để trống nếu muốn giữ nguyên số sao cũ:", "");
  let ratingStarVal = null;
  if (newStarStr && newStarStr.trim()) {
    const parsed = parseInt(newStarStr.trim());
    if (isNaN(parsed) || parsed < 1 || parsed > 5) {
      alert("Điểm đánh giá phải là số từ 1 đến 5.");
      return;
    }
    ratingStarVal = parsed;
  }

  try {
    const response = await fetch("/Product/EditComment", {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
        "X-Requested-With": "XMLHttpRequest"
      },
      body: `commentId=${commentId}&content=${encodeURIComponent(trimmed)}${ratingStarVal !== null ? `&ratingStar=${ratingStarVal}` : ''}`
    });

    const data = await response.json();
    if (data.success) {
      alert("Cập nhật đánh giá thành công!");
      window.location.reload();
    } else {
      alert(data.message || "Không thể cập nhật đánh giá.");
    }
  } catch (err) {
    alert("Lỗi kết nối khi cập nhật đánh giá. Vui lòng thử lại.");
  }
}

async function deleteReview(commentId) {
  if (!confirm("Bạn có chắc chắn muốn xóa bài đánh giá này không?")) return;

  try {
    const response = await fetch("/Product/DeleteComment", {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
        "X-Requested-With": "XMLHttpRequest"
      },
      body: `commentId=${commentId}`
    });

    const data = await response.json();
    if (data.success) {
      alert("Đã xóa đánh giá thành công!");
      window.location.reload();
    } else {
      alert(data.message || "Không thể xóa đánh giá.");
    }
  } catch (err) {
    alert("Lỗi kết nối khi xóa đánh giá. Vui lòng thử lại.");
  }
}

function escapeJsString(str) {
  if (!str) return '';
  return str.replace(/\\/g, '\\\\')
            .replace(/'/g, "\\'")
            .replace(/"/g, '\\"')
            .replace(/\n/g, '\\n')
            .replace(/\r/g, '\\r');
}

loadReviewFeed();
