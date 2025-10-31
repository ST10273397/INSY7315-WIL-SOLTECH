INSERT INTO Submissions (Title, Notes, UploadDate, Grade, Status, SubjectId, UserId)
VALUES

('Calculus Homework', 'Check the integration section.', GETDATE(), 78.5, 'Graded', 1, 'stu-001'),
('Physics Lab 1', 'Needs more detail in observations.', GETDATE(), 90.0, 'Graded', 2, 'stu-001'),

('Math Project', 'Excellent presentation.', GETDATE(), 92.0, 'Graded', 1, 'stu-001'),
('Physics Homework', '', GETDATE(), 0, 'Pending', 2, 'stu-001');