const express = require('express');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');

const app = express();
app.use(express.json());

const JWT_SECRET = process.env.JWT_SECRET || 'dev-secret-change-me-in-production';
const PORT = parseInt(process.env.PORT || '3000', 10);

// Хранилище пользователей только в памяти (демо). В продакшене — БД.
const users = [];

// Регистрация
app.post('/api/auth/register', async (req, res) => {
  const { username, password } = req.body;

  if (!username || !password) {
    return res.status(400).json({ error: 'username and password are required' });
  }

  if (users.find((u) => u.username === username)) {
    return res.status(409).json({ error: 'user already exists' });
  }

  const passwordHash = await bcrypt.hash(password, 10);
  const user = { id: users.length + 1, username, passwordHash };
  users.push(user);

  res.status(201).json({ id: user.id, username: user.username });
});

// Логин — выдаёт JWT
app.post('/api/auth/login', async (req, res) => {
  const { username, password } = req.body;

  const user = users.find((u) => u.username === username);
  if (!user || !(await bcrypt.compare(password, user.passwordHash))) {
    return res.status(401).json({ error: 'invalid credentials' });
  }

  const token = jwt.sign({ sub: user.id, username: user.username }, JWT_SECRET, {
    expiresIn: '12h',
  });

  res.json({ token, user: { id: user.id, username: user.username } });
});

// Проверка токена и получение текущего пользователя
function authenticate(req, res, next) {
  const header = req.headers.authorization || '';
  const parts = header.split(' ');
  if (parts.length !== 2 || parts[0] !== 'Bearer') {
    return res.status(401).json({ error: 'missing token' });
  }

  try {
    req.user = jwt.verify(parts[1], JWT_SECRET);
    next();
  } catch {
    res.status(401).json({ error: 'invalid or expired token' });
  }
}

app.get('/api/auth/me', authenticate, (req, res) => {
  const user = users.find((u) => u.id === req.user.sub);
  if (!user) {
    return res.status(404).json({ error: 'user not found' });
  }
  res.json({ id: user.id, username: user.username });
});

app.get('/health', (_req, res) => res.json({ status: 'ok', service: 'auth' }));

app.listen(PORT, '0.0.0.0', () => {
  console.log(`auth-service listening on :${PORT}`);
});
