let currentPreview = '';

  /* ── CHIP FILTER ── */
  function setChip(el, status) {
    document.querySelectorAll('.fchip').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
    document.querySelectorAll('.t-row').forEach(row => {
      if (status === 'all') {
        row.style.display = '';
      } else {
        row.style.display = row.dataset.status === status ? '' : 'none';
      }
    });
  }

  /* ── CHECKBOX ── */
  function toggleAll(cb) {
    document.querySelectorAll('.rck').forEach(c => c.checked = cb.checked);
    updBulk();
  }
  function updBulk() {
    const n = document.querySelectorAll('.rck:checked').length;
    const bar = document.getElementById('bulkBar');
    bar.classList.toggle('show', n > 0);
    document.getElementById('bulkInfo').textContent = `${n} bài được chọn`;
  }
  function clearSel() {
    document.querySelectorAll('.rck').forEach(c => c.checked = false);
    document.getElementById('ckAll').checked = false;
    updBulk();
  }

  /* ── EDIT MODAL ── */
  const statusConfig = {
    draft:    { cls: 'pill-draft',    label: '📝 Bản nháp' },
    pending:  { cls: 'pill-pending',  label: '⏳ Đang chờ duyệt' },
    approved: { cls: 'pill-approved', label: '✓ Đã duyệt' },
    rejected: { cls: 'pill-rejected', label: '✕ Đã từ chối' },
  };
  function openEdit(title, status, cat) {
    document.getElementById('editTitle').value = title;
    document.getElementById('editCat').value = cat;
    const cfg = statusConfig[status] || statusConfig.draft;
    document.getElementById('editStatusPill').innerHTML =
      `<span class="status-pill ${cfg.cls}">${cfg.label}</span>`;
    const subMap = {
      draft: 'Bản nháp — lưu hoặc gửi duyệt khi hoàn tất.',
      pending: 'Đang chờ duyệt — bạn chỉ có thể xem, không thể gửi lại.',
      approved: 'Bài đã duyệt — chỉnh sửa sẽ tạo phiên bản mới chờ duyệt lại.',
      rejected: 'Bài bị từ chối — hãy chỉnh sửa theo yêu cầu rồi gửi duyệt lại.',
    };
    document.getElementById('editModalSub').textContent = subMap[status] || '';
    // auto-save simulation
    startAutoSave();
    document.getElementById('editModal').classList.add('open');
  }

  let autoSaveTimer;
  function startAutoSave() {
    clearInterval(autoSaveTimer);
    autoSaveTimer = setInterval(() => {
      const el = document.getElementById('editAutoSave');
      el.textContent = '💾 Đã lưu nháp ' + new Date().toLocaleTimeString('vi-VN', {hour:'2-digit',minute:'2-digit'});
    }, 8000);
  }

  function saveDraft() {
    const el = document.getElementById('editAutoSave');
    el.textContent = '💾 Đã lưu nháp lúc ' + new Date().toLocaleTimeString('vi-VN', {hour:'2-digit',minute:'2-digit'});
    el.style.color = '#1bcf8a';
    setTimeout(() => el.style.color = 'var(--muted)', 3000);
  }

  function openSendFromEdit() {
    const title = document.getElementById('editTitle').value || 'Bài viết chưa đặt tên';
    closeModal('editModal');
    openSend(title);
  }

  function addAffRow() {
    const box = document.getElementById('editAffBox');
    const partners = ['Shopee Affiliate', 'Lazada Partner', 'Tiki Trading'];
    const p = partners[Math.floor(Math.random() * partners.length)];
    const div = document.createElement('div');
    div.className = 'aff-row';
    div.innerHTML = `<div class="aff-icon">🔗</div><div style="flex:1;"><div class="aff-name">${p}</div><input type="text" placeholder="Nhập affiliate URL..." style="width:100%;background:transparent;border:none;border-bottom:1px solid rgba(76,84,170,.18);font-size:11px;color:var(--muted);font-family:'Poppins',sans-serif;padding:2px 0;outline:none;"></div><button class="aff-del" onclick="this.closest('.aff-row').remove()">✕</button>`;
    box.appendChild(div);
  }

  /* ── SEND MODAL ── */
  function openSend(title) {
    document.getElementById('sendPostName').textContent = title;
    document.getElementById('sendModal').classList.add('open');
  }
  function confirmSend() {
    alert('✅ Đã gửi bài lên duyệt thành công!\nSuper Admin sẽ xem xét và phản hồi sớm nhất.');
    closeModal('sendModal');
  }

  /* ── DELETE MODAL ── */
  function openDelete(title) {
    document.getElementById('deletePostName').textContent = title;
    document.getElementById('deleteModal').classList.add('open');
  }
  function confirmDelete() {
    alert('🗑️ Đã xóa bài viết.');
    closeModal('deleteModal');
  }

  /* ── PREVIEW MODAL ── */
  const thumbMap = {
    'Review iPhone 16 Pro Max': 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=1200&q=80',
    'So sánh AirPods Pro 2':    'https://images.unsplash.com/photo-1546435770-a3e426bf472b?auto=format&fit=crop&w=1200&q=80',
    'Top 5 Laptop Gaming':      'https://images.unsplash.com/photo-1593642702821-c8da6771f0c6?auto=format&fit=crop&w=1200&q=80',
    'Apple Watch Series 10':    'https://images.unsplash.com/photo-1579586337278-3befd40fd17a?auto=format&fit=crop&w=1200&q=80',
    'Gaming PC Build':          'https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80',
  };
  const noticeMap = {
    draft:    { bg:'rgba(138,147,184,.08)', border:'rgba(138,147,184,.2)', color:'#5d6897', text:'Bài đang ở bản nháp. Hoàn thiện và gửi duyệt khi sẵn sàng.' },
    pending:  { bg:'rgba(245,158,11,.07)',  border:'rgba(245,158,11,.2)',  color:'#92640a', text:'Bài đang chờ Super Admin xét duyệt. Vui lòng chờ phản hồi.' },
    approved: { bg:'rgba(27,207,138,.07)',  border:'rgba(27,207,138,.2)',  color:'#0a6644', text:'Bài đã được duyệt và đang hiển thị công khai trên FTECH.' },
    rejected: { bg:'rgba(239,68,68,.07)',   border:'rgba(239,68,68,.2)',   color:'#8b1a1a', text:'Bài bị từ chối. Vui lòng chỉnh sửa theo phản hồi rồi gửi lại.' },
  };

  function openPreview(title, status, cat) {
    currentPreview = title;
    document.getElementById('pvHeadTitle').textContent = title;
    document.getElementById('pvTitle').textContent = title;
    document.getElementById('pvTag').textContent = cat;
    // thumb
    const thumbEl = document.getElementById('pvThumb');
    const thumbKey = Object.keys(thumbMap).find(k => title.includes(k.split(' ').slice(0,2).join(' ')));
    thumbEl.src = thumbKey ? thumbMap[thumbKey] : '';
    thumbEl.style.display = thumbKey ? 'block' : 'none';
    // status
    const cfg2 = statusConfig[status] || statusConfig.draft;
    document.getElementById('pvStatus').innerHTML = `<span class="status-pill ${cfg2.cls}" style="font-size:11px;padding:3px 9px;">${cfg2.label}</span>`;
    // notice
    const n = noticeMap[status] || noticeMap.draft;
    const noticeEl = document.getElementById('pvNotice');
    noticeEl.style.background = n.bg;
    noticeEl.style.borderColor = n.border;
    noticeEl.style.color = n.color;
    document.getElementById('pvNoticeText').textContent = n.text;
    // footer buttons
    const sendBtn = document.getElementById('pvSendBtn');
    sendBtn.style.display = (status === 'draft' || status === 'rejected') ? '' : 'none';
    document.getElementById('previewModal').classList.add('open');
  }

  /* ── CLOSE ── */
  function closeModal(id) {
    document.getElementById(id).classList.remove('open');
    if (id === 'editModal') clearInterval(autoSaveTimer);
  }

  /* Close on overlay click */
  document.querySelectorAll('.modal-overlay').forEach(m =>
    m.addEventListener('click', function(e) {
      if (e.target === this) closeModal(this.id);
    })
  );

  /* Toolbar toggle */
  document.querySelectorAll('.tool-btn').forEach(b =>
    b.addEventListener('click', () => b.classList.toggle('active'))
  );

  /* Pagination */
  document.querySelectorAll('.pag-btn').forEach(b =>
    b.addEventListener('click', function() {
      this.closest('.pag-btns').querySelectorAll('.pag-btn').forEach(x => x.classList.remove('active'));
      this.classList.add('active');
    })
  );
