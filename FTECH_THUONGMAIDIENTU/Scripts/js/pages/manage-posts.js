function setChip(el){document.querySelectorAll('.fchip').forEach(c=>c.classList.remove('active'));el.classList.add('active');}
function toggleAll(cb){document.querySelectorAll('.rck').forEach(c=>{c.checked=cb.checked;});updBulk();}
function updBulk(){const n=document.querySelectorAll('.rck:checked').length;const bar=document.getElementById('bulkBar');bar.classList.toggle('show',n>0);document.getElementById('bulkInfo').textContent=`${n} bài được chọn`;}
function clearSel(){document.querySelectorAll('.rck').forEach(c=>c.checked=false);document.getElementById('ckAll').checked=false;updBulk();}
function openReject(name){document.getElementById('rejectPostName').textContent=name;document.getElementById('rejectModal').classList.add('open');}
function closeModal(id){document.getElementById(id).classList.remove('open');}
function confirmReject(){alert('Đã gửi lý do từ chối đến tác giả.');closeModal('rejectModal');}
function doApprove(){alert('Bài viết đã được duyệt và xuất bản.');}
function openPreview(){document.getElementById('previewModal').classList.add('open');}
function openEditPost(title,author,category,status){document.getElementById('editPostRef').textContent=title;document.getElementById('editPostTitle').value=title;document.getElementById('editPostAuthor').value=author;document.getElementById('editPostCategory').value=category;document.getElementById('editPostStatus').value=status;document.getElementById('editPostModal').classList.add('open');}
function savePostUpdate(){alert('Đã lưu cập nhật bài viết.');closeModal('editPostModal');}
document.querySelectorAll('.modal-overlay').forEach(m=>m.addEventListener('click',function(e){if(e.target===this)this.classList.remove('open');}));
document.querySelectorAll('.pag-btn').forEach(b=>b.addEventListener('click',function(){this.closest('.pag-btns').querySelectorAll('.pag-btn').forEach(x=>x.classList.remove('active'));this.classList.add('active');}));
