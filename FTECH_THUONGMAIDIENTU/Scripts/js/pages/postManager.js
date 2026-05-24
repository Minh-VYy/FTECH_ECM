function toggleChip(el){el.classList.toggle('active');}
let previewVisible=false;
function togglePreview(){previewVisible=!previewVisible;document.getElementById('previewCard').style.display=previewVisible?'block':'none';}
function updatePreview(){const title=document.getElementById('postTitle').value||'Tiêu đề bài viết...';const excerpt=document.getElementById('postExcerpt').value||'Mô tả bài viết sẽ hiển thị ở đây...';document.getElementById('pv-title').textContent=title;document.getElementById('pv-excerpt').textContent=excerpt;}
function updateSEO(){const t=document.getElementById('seoTitle');const d=document.getElementById('seoDesc');document.getElementById('seoTitleCount').textContent=`${t.value.length}/60`;document.getElementById('seoDescCount').textContent=`${d.value.length}/160`;if(t.value)document.getElementById('seo-pv-title').textContent=t.value;if(d.value)document.getElementById('seo-pv-desc').textContent=d.value;}
function updateStatus(val){const map={draft:'si-draft',pending:'si-pending',published:'si-published'};const text={draft:'Bản nháp',pending:'Chờ duyệt',published:'Đã đăng'};const el=document.querySelector('.status-indicator');el.className=`status-indicator ${map[val]}`;el.textContent=text[val];}
document.querySelectorAll('.tool-btn').forEach(b=>b.addEventListener('click',()=>b.classList.toggle('active')));
