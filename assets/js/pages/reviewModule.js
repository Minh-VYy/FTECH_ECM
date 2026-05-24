// Stars
let mainStar=0;
const starLabels=['','Rất tệ','Không hài lòng','Bình thường','Hài lòng','Tuyệt vời!'];
function setMainStar(v){
  mainStar=v;
  document.querySelectorAll('.sp-star').forEach((s,i)=>{s.classList.toggle('lit',i<v);});
  document.getElementById('starLabel').textContent=starLabels[v];
  document.getElementById('starLabel').style.color='#fbbf24';
}
function setMini(group,v){
  const container=document.querySelector(`.mini-stars[data-group="${group}"]`);
  container.querySelectorAll('.mstar').forEach((s,i)=>s.classList.toggle('lit',i<v));
}

document.getElementById('rvText').addEventListener('input',function(){
  document.getElementById('charCount').textContent=this.value.length+' / 1000 ký tự';
});

let photoCount=0;
const photoEmojis=['🖼️','📸','🔍','💡','🎯'];
function addPhoto(){
  if(photoCount>=5){alert('Tối đa 5 ảnh.');return;}
  const c=document.getElementById('uploadedImgs');
  const d=document.createElement('div');d.className='uimg';
  d.innerHTML=photoEmojis[photoCount]+'<div class="del" onclick="this.parentElement.remove();photoCount--">×</div>';
  c.appendChild(d);photoCount++;
}

function submitReview(){
  if(mainStar===0){alert('Vui lòng chọn số sao đánh giá.');return;}
  if(!document.getElementById('rvText').value.trim()){alert('Vui lòng nhập nội dung đánh giá.');return;}
  const btn=document.querySelector('.submit-btn');
  btn.disabled=true;btn.textContent='⏳ Đang gửi...';
  setTimeout(()=>{
    document.getElementById('toastSuccess').classList.add('show');
    btn.textContent='✅ Đã gửi!';
    document.getElementById('rvText').value='';
    document.getElementById('rvTitle').value='';
    document.querySelectorAll('.sp-star').forEach(s=>s.classList.remove('lit'));
    document.querySelectorAll('.mstar').forEach(s=>s.classList.remove('lit'));
    mainStar=0;photoCount=0;
    document.getElementById('uploadedImgs').innerHTML='';
    document.getElementById('starLabel').textContent='Nhấn để chọn sao';
    setTimeout(()=>{btn.disabled=false;btn.textContent='📤 Gửi đánh giá';document.getElementById('toastSuccess').classList.remove('show');},3000);
  },1000);
}

// Reviews data
const reviewsData=[
  {
    id:1,
    name:'Nguyễn Tuấn Anh',
    avatar:'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=120&q=80',
    date:'12/03/2026',
    stars:5,
    score:5,
    title:'Pin 18 giờ thật sự ấn tượng!',
    text:'Dùng cả ngày làm việc 10 tiếng vẫn còn 30% pin. Máy mỏng nhẹ, build chắc chắn và phù hợp với học tập, văn phòng. Mình thấy nội dung review của FTECH khá sát trải nghiệm thực tế.',
    helpful:47,
    hasPhotos:true,
    verified:true,
    criteria:{perf:5,build:5,value:4,service:5},
    adminReply:{name:'FTECH Support',text:'Cảm ơn Tuấn Anh đã đánh giá chi tiết. Đánh giá này đã được kiểm duyệt và hiển thị trong module người dùng.'}
  },
  {
    id:2,
    name:'Trương Thị Kiều Nhi',
    avatar:'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=120&q=80',
    date:'08/03/2026',
    stars:5,
    score:5,
    title:'Phù hợp cho học tập và thiết kế nhẹ',
    text:'Mình dùng Figma và Photoshop cơ bản thấy rất ổn. Máy nhẹ, pin tốt và màn hình đẹp. Điểm trừ là cần thêm hub nếu dùng nhiều thiết bị ngoại vi.',
    helpful:31,
    hasPhotos:true,
    verified:true,
    criteria:{perf:5,build:5,value:4,service:5}
  },
  {
    id:3,
    name:'Phạm Thái Bảo',
    avatar:'https://images.unsplash.com/photo-1504257432389-52343af06ae3?auto=format&fit=crop&w=120&q=80',
    date:'02/03/2026',
    stars:4,
    score:4.2,
    title:'Tốt nhưng nên lên 16GB RAM nếu làm dev',
    text:'Máy chạy nhanh và rất yên tĩnh. Tuy nhiên nếu dùng Docker, IntelliJ hoặc workflow nặng hơn thì mình nghĩ nên chọn bản 16GB để thoải mái hơn.',
    helpful:18,
    hasPhotos:false,
    verified:true,
    criteria:{perf:4,build:5,value:4,service:5}
  },
];

let shownCount=3;
function renderReviews(list){
  document.getElementById('reviewsList').innerHTML=list.map(r=>`
    <div class="rv-card">
      <div class="rv-top">
        <div class="rv-user"><div class="rv-avatar"><img src="${r.avatar}" alt="${r.name}"></div><div><div class="rv-name">${r.name}</div><div class="rv-meta">${r.date} · Đã mua tại FTECH</div><div class="rv-source">Nguồn ảnh minh họa: Unsplash</div></div></div>
        <div class="rv-right">
          ${r.verified?'<span class="rv-verified">✓ Đã duyệt</span>':''}
          <span class="rv-stars-row">${'⭐'.repeat(r.stars)}</span>
          <span class="rv-score">${r.score}</span>
        </div>
      </div>
      ${r.criteria?`<div class="rv-criteria"><div class="rv-crit">Hiệu năng<span>${'⭐'.repeat(r.criteria.perf)}</span></div><div class="rv-crit">Build<span>${'⭐'.repeat(r.criteria.build)}</span></div><div class="rv-crit">Giá trị<span>${'⭐'.repeat(r.criteria.value)}</span></div><div class="rv-crit">Dịch vụ<span>${'⭐'.repeat(r.criteria.service)}</span></div></div>`:''}
      <div class="rv-title">${r.title}</div>
      <div class="rv-text">${r.text}</div>
      ${r.hasPhotos?`<div class="rv-photos"><div class="rv-photo">📸</div><div class="rv-photo">🖼️</div><div class="rv-photo">🔍</div></div>`:''}
      <div class="rv-actions">
        <span class="rv-helpful">Hữu ích không?</span>
        <button class="rv-helpful-btn" onclick="vote(this,${r.id},'up')">👍 Có (${r.helpful})</button>
        <button class="rv-helpful-btn" onclick="vote(this,${r.id},'down')">👎 Không</button>
        <button class="rv-report">🚩 Báo cáo</button>
      </div>
      ${r.adminReply?`<div class="admin-reply"><div class="ar-header"><span class="ar-badge">FTECH STAFF</span><span class="ar-name">${r.adminReply.name}</span><span class="ar-time">· Phản hồi chính thức</span></div><div class="ar-text">${r.adminReply.text}</div></div>`:''}
    </div>`).join('');
}
renderReviews(reviewsData);

function vote(btn,id,dir){
  if(btn.classList.contains('voted'))return;
  btn.classList.add('voted');
}
function filterTab(btn,val){
  document.querySelectorAll('.ftab').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
}
function filterByStar(n){
  document.querySelectorAll('.ftab').forEach((b,i)=>{b.classList.toggle('active',i===n-1||i===5-n+1);});
}
function sortReviews(v){}
function loadMoreReviews(){
  shownCount+=5;
  alert('Tải thêm '+shownCount+' đánh giá từ API (demo).');
}
