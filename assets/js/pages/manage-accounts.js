function openUserModal(name,role,status,scope){document.getElementById('userModalName').value=name;document.getElementById('userModalRole').value=role;document.getElementById('userModalStatus').value=status;document.getElementById('userModalScope').value=scope;document.getElementById('userModal').classList.add('open')}
    function openLockModal(name){document.getElementById('lockModalName').textContent=name;document.getElementById('lockModal').classList.add('open')}
    function closeAccountModal(id){document.getElementById(id).classList.remove('open')}
    function saveUserProcess(){alert('Đã lưu cập nhật tài khoản admin.');closeAccountModal('userModal')}
    function submitLockAction(){alert('Đã cập nhật vai trò và quyền của tài khoản admin.');closeAccountModal('lockModal')}
    document.querySelectorAll('.modal-overlay').forEach(function(m){m.addEventListener('click',function(e){if(e.target===this)this.classList.remove('open')})})
