const API_URL = 'http://localhost:5001';
const fileInput = document.getElementById('fileInput');
const uploadStatus = document.getElementById('uploadStatus');
const videoGrid = document.getElementById('videoGrid');

fileInput.addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    uploadStatus.textContent = 'Uploading...';
    uploadStatus.style.color = '#fff';

    try {
        const response = await fetch(`${API_URL}/videos/upload`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) throw new Error('Upload failed');

        const data = await response.json();
        uploadStatus.textContent = 'Upload successful! Processing...';
        uploadStatus.style.color = '#46d369';

        addVideoToGrid(data.id, file.name, 'Pending');
        pollStatus(data.id);
    } catch (error) {
        console.error(error);
        uploadStatus.textContent = 'Error uploading file';
        uploadStatus.style.color = '#e50914';
    }
});

const modal = document.getElementById('videoModal');
const videoPlayer = document.getElementById('videoPlayer');
const closeModal = document.querySelector('.close-modal');

// Close modal logic
closeModal.onclick = () => {
    modal.style.display = 'none';
    videoPlayer.pause();
    videoPlayer.src = '';
}

window.onclick = (event) => {
    if (event.target == modal) {
        modal.style.display = 'none';
        videoPlayer.pause();
        videoPlayer.src = '';
    }
}

function playVideo(id) {
    modal.style.display = 'flex';
    videoPlayer.src = `${API_URL}/videos/${id}/stream`;
    videoPlayer.play();
}

function addVideoToGrid(id, name, status) {
    const card = document.createElement('div');
    card.className = 'video-card';
    card.id = `card-${id}`;

    const statusClass = status.toLowerCase();
    const playButtonHtml = status === 'Completed'
        ? `<button class="play-btn" onclick="playVideo('${id}')">▶ Play</button>`
        : '';

    card.innerHTML = `
        <div class="card-content">
            <div class="card-title" title="${name}">${name}</div>
            <div class="card-status">
                <span class="status-indicator ${statusClass}" id="indicator-${id}"></span>
                <span id="status-${id}">${status}</span>
            </div>
            <div id="actions-${id}">
                ${playButtonHtml}
            </div>
        </div>
    `;

    videoGrid.prepend(card);
}

async function pollStatus(id) {
    const statusSpan = document.getElementById(`status-${id}`);
    const indicator = document.getElementById(`indicator-${id}`);
    const actionsDiv = document.getElementById(`actions-${id}`);

    const interval = setInterval(async () => {
        try {
            const response = await fetch(`${API_URL}/videos/${id}`);
            const data = await response.json();

            statusSpan.textContent = data.status;
            indicator.className = `status-indicator ${data.status.toLowerCase()}`;

            if (data.status === 'Completed') {
                clearInterval(interval);
                // Add play button if not already there
                if (!actionsDiv.querySelector('.play-btn')) {
                    actionsDiv.innerHTML = `<button class="play-btn" onclick="playVideo('${id}')">▶ Play</button>`;
                }
            }
        } catch (error) {
            console.error('Polling error:', error);
        }
    }, 2000);
}
