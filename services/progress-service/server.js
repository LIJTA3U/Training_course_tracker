const express = require('express');
const app = express();
app.use(express.json());

const PORT = parseInt(process.env.PORT || '3000', 10);

// Хранилище только в памяти (демо). В продакшене — БД.
const enrollments = []; // { userId, courseId, enrolledAt }
const lessons = []; // { userId, courseId, lessonId, completed, updatedAt }

// Курсы, на которые записан пользователь
app.get('/api/progress/courses', (req, res) => {
  const { userId } = req.query;
  if (!userId) {
    return res.status(400).json({ error: 'userId is required' });
  }

  const userEnrollments = enrollments.filter((e) => e.userId === userId);
  res.json(userEnrollments);
});

// Запись на курс
app.post('/api/progress/enroll', (req, res) => {
  const { userId, courseId } = req.body;
  if (!userId || !courseId) {
    return res.status(400).json({ error: 'userId and courseId are required' });
  }

  if (enrollments.some((e) => e.userId === userId && e.courseId === courseId)) {
    return res.status(409).json({ error: 'already enrolled in this course' });
  }

  const enrollment = { userId, courseId, enrolledAt: new Date().toISOString() };
  enrollments.push(enrollment);
  res.status(201).json(enrollment);
});

// Отметка урока как пройденного / не пройденного
app.post('/api/progress/lesson', (req, res) => {
  const { userId, courseId, lessonId, completed } = req.body;
  if (!userId || !courseId || !lessonId) {
    return res.status(400).json({ error: 'userId, courseId and lessonId are required' });
  }

  const idx = lessons.findIndex(
    (l) => l.userId === userId && l.courseId === courseId && l.lessonId === lessonId,
  );

  const record = {
    userId,
    courseId,
    lessonId,
    completed: completed === true,
    updatedAt: new Date().toISOString(),
  };

  if (idx === -1) {
    lessons.push(record);
  } else {
    lessons[idx] = record;
  }

  res.json(record);
});

// Полный прогресс пользователя
app.get('/api/progress/:userId', (req, res) => {
  const { userId } = req.params;

  const userLessons = lessons.filter((l) => l.userId === userId);
  const enrolledCourseIds = enrollments
    .filter((e) => e.userId === userId)
    .map((e) => e.courseId);

  res.json({ userId, enrolledCourses: enrolledCourseIds, lessons: userLessons });
});

app.get('/health', (_req, res) => res.json({ status: 'ok', service: 'progress' }));

app.listen(PORT, '0.0.0.0', () => {
  console.log(`progress-service listening on :${PORT}`);
});
