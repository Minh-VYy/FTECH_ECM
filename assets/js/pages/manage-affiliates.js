function setF(el){document.querySelectorAll('.filter-chip').forEach(c=>c.classList.remove('active'));el.classList.add('active');}

function openAffiliateModal(mode,name=''){const title=document.getElementById('affiliateModalTitle');document.getElementById('affiliateNameField').value=name;if(mode==='create'){title.textContent='Tạo link affiliate mới';}else if(mode==='repair'){title.textContent='Sửa link affiliate bị lỗi';}else{title.textContent='Cập nhật affiliate link';}document.getElementById('affiliateModal').classList.add('open');}
function openCommissionModal(partner=''){if(partner){document.getElementById('commissionPartnerField').value=partner;}document.getElementById('commissionModal').classList.add('open');}
function closeAffiliateModal(id){document.getElementById(id).classList.remove('open');}
function saveAffiliateLink(){alert('Đã lưu affiliate link.');closeAffiliateModal('affiliateModal');}
function saveCommission(){alert('Đã lưu tỷ lệ hoa hồng.');closeAffiliateModal('commissionModal');}
document.querySelectorAll('.modal-overlay').forEach(m=>m.addEventListener('click',function(e){if(e.target===this)this.classList.remove('open');}));
