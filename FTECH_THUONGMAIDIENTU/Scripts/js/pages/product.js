function buyProduct(postId, partnerId, affiliateUrl) {
    // 1. Open buy redirect action in a new tab
    const buyUrl = `/Product/Buy?postId=${postId}&partnerId=${partnerId}&url=${encodeURIComponent(affiliateUrl)}`;
    window.open(buyUrl, '_blank');

    // 2. Set click record in localStorage for real-time frontend unlock
    localStorage.setItem(`clicked_${postId}`, "true");

    // 3. Smoothly unlock the comment form in real time
    const lockBox = document.getElementById("commentLockBox");
    const commentForm = document.getElementById("commentForm");
    if (lockBox && commentForm) {
        lockBox.style.display = "none";
        commentForm.style.display = "block";
    }
}

async function submitRealComment(postId) {
    const input = document.getElementById("commentInput");
    const value = input.value.trim();

    if (!value) {
        alert("Vui lòng nhập nội dung bình luận đánh giá của bạn.");
        return;
    }

    // Call C# Backend AddComment Action via AJAX (form urlencoded POST)
    try {
        const response = await fetch("/Product/AddComment", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: `postId=${postId}&content=${encodeURIComponent(value)}`
        });

        if (!response.ok) {
            throw new Error("Mất kết nối với máy chủ.");
        }

        const data = await response.json();

        if (data.success) {
            // Success: Prepend real comment dynamically
            const commentList = document.getElementById("commentList");
            
            // Remove the empty comment message if it exists
            if (commentList.textContent.includes("Chưa có bình luận nào")) {
                commentList.innerHTML = "";
            }

            const block = document.createElement("div");
            block.className = "comment-item";

            const head = document.createElement("div");
            head.className = "comment-head";

            const nameDiv = document.createElement("div");
            nameDiv.className = "comment-name";
            nameDiv.style.display = "flex";
            nameDiv.style.alignItems = "center";
            nameDiv.style.gap = "8px";

            const avatarImg = document.createElement("img");
            const avatarPath = data.memberAvatar ? `/Content/images/${data.memberAvatar}` : "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=80&q=80";
            avatarImg.src = avatarPath;
            avatarImg.style.width = "32px";
            avatarImg.style.height = "32px";
            avatarImg.style.borderRadius = "50%";
            avatarImg.style.objectFit = "cover";
            avatarImg.style.border = "1px solid var(--border)";

            const nameSpan = document.createElement("span");
            nameSpan.textContent = data.memberName || "Thành viên";

            nameDiv.append(avatarImg, nameSpan);

            const date = document.createElement("div");
            date.className = "comment-date";
            date.textContent = "Vừa xong";

            head.append(nameDiv, date);

            const text = document.createElement("div");
            text.className = "comment-text";
            text.style.paddingLeft = "40px";
            text.style.marginTop = "4px";
            text.textContent = value;

            block.append(head, text);
            commentList.prepend(block);
            
            input.value = "";
        } else {
            alert(data.message || "Có lỗi xảy ra, vui lòng thử lại.");
        }
    } catch (err) {
        alert(err.message || "Lỗi gửi bình luận. Vui lòng kiểm tra lại kết nối.");
    }
}

// Check localStorage on page load to unlock comment section immediately if already clicked in this browser session
document.addEventListener("DOMContentLoaded", () => {
    // Get current post id
    const commentForm = document.getElementById("commentForm");
    if (commentForm) {
        // Find PostID by helper or parsing submit click argument
        const submitBtn = commentForm.querySelector("button");
        if (submitBtn) {
            const match = submitBtn.getAttribute("onclick").match(/\d+/);
            if (match) {
                const postId = match[0];
                if (localStorage.getItem(`clicked_${postId}`) === "true") {
                    const lockBox = document.getElementById("commentLockBox");
                    if (lockBox) {
                        lockBox.style.display = "none";
                        commentForm.style.display = "block";
                    }
                }
            }
        }
    }
});
