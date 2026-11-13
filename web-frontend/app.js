(() => {
  const API_BASE = ''; 

  const state = {
    token: localStorage.getItem('ct_token') || '',
    userId: null,
    username: '',
  };

  const authSection = document.getElementById('auth');
  const coursesSection = document.getElementById('courses');
  const progressSection = document.getElementById('progress');

  const btnRegister = document.getElementById('btnRegister');
  const btnLogin = document.getElementById('btnLogin');
  const btnLogout = document.getElementById('btnLogout');
  const authMsg = document.getElementById('authMsg');
  const userInfo = document.getElementById('userInfo');

  const coursesList = document.getElementById('coursesList');
  const coursesMsg = document.getElementById('coursesMsg');
  const progressList = document.getElementById('progressList');
  const progressMsg = document.getElementById('progressMsg');

  function setMsg(el, text, ok = false) {
    el.textContent = text;
    el.className = 'msg ' + (ok ? 'ok' : '');
  }

  function authHeaders() {
    return state.token ? { Authorization: `Bearer ${state.token}` } : {};
  }

  async function api(path, options = {}) {
    const res = await fetch(`${API_BASE}${path}`, {
      headers: { 'Content-Type': 'application/json', ...authHeaders(), ...options.headers },
      ...options,
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.error || `Request failed (${res.status})`);
    return data;
  }

  function showAuth() {
    authSection.style.display = 'block';
    coursesSection.style.display = 'none';
    progressSection.style.display = 'none';
    btnLogout.style.display = 'none';
    userInfo.textContent = '';
  }

  function showApp() {
    authSection.style.display = 'none';
    coursesSection.style.display = 'block';
    progressSection.style.display = 'block';
    btnLogout.style.display = 'inline-block';
    userInfo.textContent = `Вы вошли как ${state.username}`;
    loadCourses();
    loadProgress();
  }

  btnRegister.addEventListener('click', async () => {
    const username = document.getElementById('regUsername').value.trim();
    const password = document.getElementById('regPassword').value;
    try {
      await api('/api/auth/register', { method: 'POST', body: JSON.stringify({ username, password }) });
      setMsg(authMsg, 'Регистрация прошла успешно. Теперь войдите.', true);
    } catch (err) {
      setMsg(authMsg, err.message);
    }
  });

  btnLogin.addEventListener('click', async () => {
    const username = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value;
    try {
      const { token, user } = await api('/api/auth/login', { method: 'POST', body: JSON.stringify({ username, password }) });
      state.token = token;
      state.userId = user.id;
      state.username = user.username;
      localStorage.setItem('ct_token', token);
      showApp();
    } catch (err) {
      setMsg(authMsg, err.message);
    }
  });

  btnLogout.addEventListener('click', () => {
    state.token = '';
    state.userId = null;
    state.username = '';
    localStorage.removeItem('ct_token');
    showAuth();
  });

  if (state.token) {
    api('/api/auth/me').then((user) => {
      state.userId = user.id;
      state.username = user.username;
      showApp();
    }).catch(() => showAuth());
  } else {
    showAuth();
  }

  async function loadCourses() {
    try {
      const courses = await api('/api/courses');
      coursesList.innerHTML = courses.map((c) => {
        const pct = c.TotalHours ? Math.round((c.CompletedHours / c.TotalHours) * 100) : 0;
        return `
          <div class="course-card">
            <h3>${escapeHtml(c.Name)}</h3>
            <div class="meta">${c.CompletedHours} / ${c.TotalHours} часов${c.Completed ? ' — пройден' : ''}</div>
            <div class="progress-bar"><div class="progress-fill" style="width:${pct}%"></div></div>
            <button data-id="${c.Id}">${c.Completed ? 'Уже пройден' : 'Записаться'}</button>
          </div>
        `;
      }).join('');

      coursesList.querySelectorAll('button').forEach((btn) => {
        btn.addEventListener('click', async () => {
          const courseId = btn.dataset.id;
          try {
            await api('/api/progress/enroll', {
              method: 'POST',
              body: JSON.stringify({ userId: state.userId, courseId: Number(courseId) }),
            });
            setMsg(coursesMsg, 'Запись оформлена', true);
            loadProgress();
          } catch (err) {
            if (err.message.includes('already enrolled')) {
              setMsg(coursesMsg, 'Уже записан на этот курс', true);
              loadProgress();
            } else {
              setMsg(coursesMsg, err.message);
            }
          }
        });
      });
    } catch (err) {
      setMsg(coursesMsg, err.message);
    }
  }

  async function loadProgress() {
    try {
      const data = await api(`/api/progress/${state.userId}`);
      if (data.enrolledCourses.length === 0) {
        progressList.innerHTML = '<p style="color:#6b7a8d">Вы пока не записаны ни на один курс.</p>';
        return;
      }

      progressList.innerHTML = data.enrolledCourses.map((courseId) => {
        const courseLessons = data.lessons.filter((l) => l.courseId === Number(courseId));
        const done = courseLessons.filter((l) => l.completed).length;
        const total = courseLessons.length || 1;
        const pct = Math.round((done / total) * 100);
        return `
          <div class="progress-item">
            <div>
              <div class="label">Курс #${escapeHtml(courseId)}</div>
              <div style="font-size:12px;color:#6b7a8d">Уроков пройдено: ${done} / ${total}</div>
            </div>
            <div class="value">${pct}%</div>
          </div>
        `;
      }).join('');
    } catch (err) {
      setMsg(progressMsg, err.message);
    }
  }

  function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }
})();