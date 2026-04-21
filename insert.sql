-- StadiumCompany - Demo Data
-- Mot de passe admin : adminadmin
-- Mot de passe autres comptes : password

-- ============================================================
-- RESET (supprime toutes les donnees existantes)
-- ============================================================

TRUNCATE TABLE
    activity_logs,
    question_feedbacks,
    quiz_answers,
    quiz_submissions,
    answers,
    questions,
    questionnaires,
    user_preferences,
    users,
    themes
RESTART IDENTITY CASCADE;


-- ============================================================
-- THEMES
-- ============================================================

INSERT INTO themes (id, name) VALUES
    (1, 'Development'),
    (2, 'Network'),
    (3, 'General knowledge'),
    (4, 'Security'),
    (5, 'Database'),
    (6, 'DevOps');


-- ============================================================
-- USERS
-- ============================================================

INSERT INTO users (id, email, password, last_name, first_name, is_admin, is_archived, created_at) VALUES
    (1, 'admin@stadiumcompany.com',          '$2y$10$/nvNYCm/VH6ZoNWC4GJa4u7GKTSt62/KGJPKTvFCP5VG8BfRTuouG', 'Martin',  'Alice',  TRUE,  FALSE, '2026-01-10 09:00:00'),
    (2, 'jean.dupont@stadiumcompany.com',     '$2y$10$BX109ltecvSFV0gGASnCRenknR5Nh6LTw9A4vLplpMVA/iX8L0q96', 'Dupont',  'Jean',   FALSE, FALSE, '2026-01-15 14:30:00'),
    (3, 'marie.leroy@stadiumcompany.com',     '$2y$10$BX109ltecvSFV0gGASnCRenknR5Nh6LTw9A4vLplpMVA/iX8L0q96', 'Leroy',   'Marie',  FALSE, FALSE, '2026-02-01 10:00:00'),
    (4, 'thomas.bernard@stadiumcompany.com',  '$2y$10$BX109ltecvSFV0gGASnCRenknR5Nh6LTw9A4vLplpMVA/iX8L0q96', 'Bernard', 'Thomas', FALSE, FALSE, '2026-02-20 16:45:00'),
    (5, 'sophie.moreau@stadiumcompany.com',   '$2y$10$BX109ltecvSFV0gGASnCRenknR5Nh6LTw9A4vLplpMVA/iX8L0q96', 'Moreau',  'Sophie', FALSE, TRUE,  '2025-06-05 11:00:00');


-- ============================================================
-- USER PREFERENCES
-- ============================================================

INSERT INTO user_preferences (user_id, theme, language) VALUES
    (1, 'Light', 'fr'),
    (2, 'Dark',  'fr'),
    (3, 'Light', 'en'),
    (4, 'Dark',  'fr'),
    (5, 'Light', 'fr');


-- ============================================================
-- QUESTIONNAIRES
-- ============================================================

INSERT INTO questionnaires (id, name, theme_id, user_id, question_count, published) VALUES
    (1, 'Les bases de HTML et CSS',       1, 1, 5, TRUE),
    (2, 'Fondamentaux des reseaux',        2, 2, 5, TRUE),
    (3, 'Securite informatique 101',       4, 1, 5, TRUE),
    (4, 'SQL et bases de donnees',         5, 3, 5, TRUE),
    (5, 'Culture generale IT',             3, 2, 4, TRUE),
    (6, 'Introduction a Docker',           6, 4, 4, FALSE),
    (7, 'JavaScript avance',               1, 3, 3, FALSE),
    (8, 'Protocoles reseau avances',       2, 1, 4, TRUE);


-- ============================================================
-- QUESTIONS
-- ============================================================

-- Questionnaire 1 : Les bases de HTML et CSS
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (1,  1, 1, 'HTML est un langage de programmation.',                              'TRUE_FALSE'),
    (2,  1, 2, 'Quelle balise HTML est utilisee pour creer un lien hypertexte ?',   'MULTIPLE_CHOICE'),
    (3,  1, 3, 'Quelle propriete CSS permet de modifier la couleur du texte ?',      'MULTIPLE_CHOICE'),
    (4,  1, 4, 'CSS signifie Cascading Style Sheets.',                               'TRUE_FALSE'),
    (5,  1, 5, 'Quel element HTML5 definit une section de navigation ?',             'MULTIPLE_CHOICE');

-- Questionnaire 2 : Fondamentaux des reseaux
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (6,  2, 1, 'Quel protocole est utilise pour envoyer des emails ?',               'MULTIPLE_CHOICE'),
    (7,  2, 2, 'Une adresse IPv4 est composee de 128 bits.',                         'TRUE_FALSE'),
    (8,  2, 3, 'Quel port est utilise par defaut pour HTTPS ?',                      'MULTIPLE_CHOICE'),
    (9,  2, 4, 'Quelle couche du modele OSI gere le routage ?',                      'MULTIPLE_CHOICE'),
    (10, 2, 5, 'Le protocole TCP garantit la livraison des paquets.',                'TRUE_FALSE');

-- Questionnaire 3 : Securite informatique 101
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (11, 3, 1, 'Quel type d''attaque consiste a envoyer un email frauduleux pour voler des informations ?', 'MULTIPLE_CHOICE'),
    (12, 3, 2, 'Un pare-feu protege uniquement contre les virus.',                   'TRUE_FALSE'),
    (13, 3, 3, 'Que signifie HTTPS ?',                                               'MULTIPLE_CHOICE'),
    (14, 3, 4, 'Le chiffrement symetrique utilise la meme cle pour chiffrer et dechiffrer.', 'TRUE_FALSE'),
    (15, 3, 5, 'Quelle est la longueur minimale recommandee pour un mot de passe securise ?', 'MULTIPLE_CHOICE');

-- Questionnaire 4 : SQL et bases de donnees
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (16, 4, 1, 'Quelle commande SQL permet de recuperer des donnees ?',              'MULTIPLE_CHOICE'),
    (17, 4, 2, 'Une cle primaire peut contenir des valeurs NULL.',                   'TRUE_FALSE'),
    (18, 4, 3, 'Quel type de jointure retourne uniquement les lignes correspondantes des deux tables ?', 'MULTIPLE_CHOICE'),
    (19, 4, 4, 'Quelles sont des proprietes ACID d''une transaction ?',              'MULTIPLE_CHOICE'),
    (20, 4, 5, 'L''instruction DELETE sans clause WHERE supprime toutes les lignes de la table.', 'TRUE_FALSE');

-- Questionnaire 5 : Culture generale IT
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (21, 5, 1, 'Qui est considere comme le pere de l''informatique ?',               'MULTIPLE_CHOICE'),
    (22, 5, 2, 'Linux est un systeme d''exploitation proprietaire.',                 'TRUE_FALSE'),
    (23, 5, 3, 'En quelle annee le World Wide Web a-t-il ete invente ?',             'MULTIPLE_CHOICE'),
    (24, 5, 4, 'Quel langage est principalement utilise pour le developpement iOS natif ?', 'MULTIPLE_CHOICE');

-- Questionnaire 6 : Introduction a Docker
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (25, 6, 1, 'Docker utilise la virtualisation complete comme les machines virtuelles.', 'TRUE_FALSE'),
    (26, 6, 2, 'Quel fichier definit la configuration d''une image Docker ?',        'MULTIPLE_CHOICE'),
    (27, 6, 3, 'Quelle commande permet de lister les conteneurs en cours d''execution ?', 'MULTIPLE_CHOICE'),
    (28, 6, 4, 'Un conteneur Docker partage le noyau du systeme hote.',              'TRUE_FALSE');

-- Questionnaire 7 : JavaScript avance
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (29, 7, 1, 'Quel mot-cle permet de declarer une variable dont la valeur ne peut pas etre reassignee ?', 'MULTIPLE_CHOICE'),
    (30, 7, 2, 'JavaScript est un langage a typage statique.',                       'TRUE_FALSE'),
    (31, 7, 3, 'Quelle methode permet de transformer un tableau en une seule valeur ?', 'MULTIPLE_CHOICE');

-- Questionnaire 8 : Protocoles reseau avances
INSERT INTO questions (id, questionnaire_id, number, label, answer_type) VALUES
    (32, 8, 1, 'Quel protocole est utilise pour la resolution de noms de domaine ?', 'MULTIPLE_CHOICE'),
    (33, 8, 2, 'Le protocole UDP est oriente connexion.',                            'TRUE_FALSE'),
    (34, 8, 3, 'Quel protocole permet d''attribuer automatiquement une adresse IP a un appareil ?', 'MULTIPLE_CHOICE'),
    (35, 8, 4, 'Quels protocoles operent au niveau de la couche transport ?',        'MULTIPLE_CHOICE');


-- ============================================================
-- ANSWERS
-- ============================================================

-- Q1 : HTML est un langage de programmation.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (1,  1, 'Vrai', FALSE, 0.00),
    (2,  1, 'Faux', TRUE,  1.00);

-- Q2 : Quelle balise HTML est utilisee pour creer un lien hypertexte ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (3,  2, '<a>',    TRUE,  1.00),
    (4,  2, '<link>', FALSE, 0.00),
    (5,  2, '<href>', FALSE, 0.00),
    (6,  2, '<url>',  FALSE, 0.00);

-- Q3 : Quelle propriete CSS permet de modifier la couleur du texte ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (7,  3, 'color',            TRUE,  1.00),
    (8,  3, 'font-color',       FALSE, 0.00),
    (9,  3, 'text-color',       FALSE, 0.00),
    (10, 3, 'background-color', FALSE, 0.00);

-- Q4 : CSS signifie Cascading Style Sheets.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (11, 4, 'Vrai', TRUE,  1.00),
    (12, 4, 'Faux', FALSE, 0.00);

-- Q5 : Quel element HTML5 definit une section de navigation ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (13, 5, '<nav>',        TRUE,  1.00),
    (14, 5, '<navigation>', FALSE, 0.00),
    (15, 5, '<menu>',       FALSE, 0.00),
    (16, 5, '<header>',     FALSE, 0.00);

-- Q6 : Quel protocole est utilise pour envoyer des emails ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (17, 6, 'SMTP', TRUE,  1.00),
    (18, 6, 'FTP',  FALSE, 0.00),
    (19, 6, 'HTTP', FALSE, 0.00),
    (20, 6, 'SSH',  FALSE, 0.00);

-- Q7 : Une adresse IPv4 est composee de 128 bits.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (21, 7, 'Vrai', FALSE, 0.00),
    (22, 7, 'Faux', TRUE,  1.00);

-- Q8 : Quel port est utilise par defaut pour HTTPS ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (23, 8, '443',  TRUE,  1.00),
    (24, 8, '80',   FALSE, 0.00),
    (25, 8, '8080', FALSE, 0.00),
    (26, 8, '22',   FALSE, 0.00);

-- Q9 : Quelle couche du modele OSI gere le routage ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (27, 9, 'Couche 3 - Reseau',    TRUE,  1.00),
    (28, 9, 'Couche 2 - Liaison',   FALSE, 0.00),
    (29, 9, 'Couche 4 - Transport', FALSE, 0.00),
    (30, 9, 'Couche 1 - Physique',  FALSE, 0.00);

-- Q10 : Le protocole TCP garantit la livraison des paquets.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (31, 10, 'Vrai', TRUE,  1.00),
    (32, 10, 'Faux', FALSE, 0.00);

-- Q11 : Quel type d'attaque consiste a envoyer un email frauduleux ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (33, 11, 'Phishing',          TRUE,  1.00),
    (34, 11, 'DDoS',              FALSE, 0.00),
    (35, 11, 'Brute force',       FALSE, 0.00),
    (36, 11, 'Man-in-the-middle', FALSE, 0.00);

-- Q12 : Un pare-feu protege uniquement contre les virus.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (37, 12, 'Vrai', FALSE, 0.00),
    (38, 12, 'Faux', TRUE,  1.00);

-- Q13 : Que signifie HTTPS ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (39, 13, 'HyperText Transfer Protocol Secure',       TRUE,  1.00),
    (40, 13, 'HyperText Transfer Protocol Standard',     FALSE, 0.00),
    (41, 13, 'High Transfer Protocol Secure',            FALSE, 0.00),
    (42, 13, 'HyperText Transmission Protocol Secure',   FALSE, 0.00);

-- Q14 : Le chiffrement symetrique utilise la meme cle pour chiffrer et dechiffrer.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (43, 14, 'Vrai', TRUE,  1.00),
    (44, 14, 'Faux', FALSE, 0.00);

-- Q15 : Quelle est la longueur minimale recommandee pour un mot de passe securise ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (45, 15, '12 caracteres', TRUE,  1.00),
    (46, 15, '4 caracteres',  FALSE, 0.00),
    (47, 15, '6 caracteres',  FALSE, 0.00),
    (48, 15, '8 caracteres',  FALSE, 0.00);

-- Q16 : Quelle commande SQL permet de recuperer des donnees ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (49, 16, 'SELECT',   TRUE,  1.00),
    (50, 16, 'GET',      FALSE, 0.00),
    (51, 16, 'FETCH',    FALSE, 0.00),
    (52, 16, 'RETRIEVE', FALSE, 0.00);

-- Q17 : Une cle primaire peut contenir des valeurs NULL.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (53, 17, 'Vrai', FALSE, 0.00),
    (54, 17, 'Faux', TRUE,  1.00);

-- Q18 : Quel type de jointure retourne uniquement les lignes correspondantes ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (55, 18, 'INNER JOIN', TRUE,  1.00),
    (56, 18, 'LEFT JOIN',  FALSE, 0.00),
    (57, 18, 'RIGHT JOIN', FALSE, 0.00),
    (58, 18, 'CROSS JOIN', FALSE, 0.00);

-- Q19 : Quelles sont des proprietes ACID d'une transaction ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (59, 19, 'Atomicite', TRUE,  1.00),
    (60, 19, 'Coherence', TRUE,  1.00),
    (61, 19, 'Rapidite',  FALSE, 0.00),
    (62, 19, 'Durabilite', TRUE,  1.00);

-- Q20 : L'instruction DELETE sans clause WHERE supprime toutes les lignes.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (63, 20, 'Vrai', TRUE,  1.00),
    (64, 20, 'Faux', FALSE, 0.00);

-- Q21 : Qui est considere comme le pere de l'informatique ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (65, 21, 'Alan Turing',    TRUE,  1.00),
    (66, 21, 'Steve Jobs',     FALSE, 0.00),
    (67, 21, 'Bill Gates',     FALSE, 0.00),
    (68, 21, 'Linus Torvalds', FALSE, 0.00);

-- Q22 : Linux est un systeme d'exploitation proprietaire.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (69, 22, 'Vrai', FALSE, 0.00),
    (70, 22, 'Faux', TRUE,  1.00);

-- Q23 : En quelle annee le World Wide Web a-t-il ete invente ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (71, 23, '1989', TRUE,  1.00),
    (72, 23, '1995', FALSE, 0.00),
    (73, 23, '1983', FALSE, 0.00),
    (74, 23, '2000', FALSE, 0.00);

-- Q24 : Quel langage est principalement utilise pour le developpement iOS natif ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (75, 24, 'Swift',  TRUE,  1.00),
    (76, 24, 'Java',   FALSE, 0.00),
    (77, 24, 'Python', FALSE, 0.00),
    (78, 24, 'C#',     FALSE, 0.00);

-- Q25 : Docker utilise la virtualisation complete comme les machines virtuelles.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (79, 25, 'Vrai', FALSE, 0.00),
    (80, 25, 'Faux', TRUE,  1.00);

-- Q26 : Quel fichier definit la configuration d'une image Docker ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (81, 26, 'Dockerfile',         TRUE,  1.00),
    (82, 26, 'docker-config.yml',  FALSE, 0.00),
    (83, 26, 'Imagefile',          FALSE, 0.00),
    (84, 26, 'container.json',     FALSE, 0.00);

-- Q27 : Quelle commande permet de lister les conteneurs en cours d'execution ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (85, 27, 'docker ps',      TRUE,  1.00),
    (86, 27, 'docker list',    FALSE, 0.00),
    (87, 27, 'docker running', FALSE, 0.00),
    (88, 27, 'docker status',  FALSE, 0.00);

-- Q28 : Un conteneur Docker partage le noyau du systeme hote.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (89, 28, 'Vrai', TRUE,  1.00),
    (90, 28, 'Faux', FALSE, 0.00);

-- Q29 : Quel mot-cle permet de declarer une variable non reassignable ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (91,  29, 'const', TRUE,  1.00),
    (92,  29, 'var',   FALSE, 0.00),
    (93,  29, 'let',   FALSE, 0.00),
    (94,  29, 'final', FALSE, 0.00);

-- Q30 : JavaScript est un langage a typage statique.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (95, 30, 'Vrai', FALSE, 0.00),
    (96, 30, 'Faux', TRUE,  1.00);

-- Q31 : Quelle methode permet de transformer un tableau en une seule valeur ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (97,  31, 'reduce()',  TRUE,  1.00),
    (98,  31, 'map()',     FALSE, 0.00),
    (99,  31, 'filter()',  FALSE, 0.00),
    (100, 31, 'forEach()', FALSE, 0.00);

-- Q32 : Quel protocole est utilise pour la resolution de noms de domaine ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (101, 32, 'DNS',  TRUE,  1.00),
    (102, 32, 'DHCP', FALSE, 0.00),
    (103, 32, 'ARP',  FALSE, 0.00),
    (104, 32, 'NAT',  FALSE, 0.00);

-- Q33 : Le protocole UDP est oriente connexion.
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (105, 33, 'Vrai', FALSE, 0.00),
    (106, 33, 'Faux', TRUE,  1.00);

-- Q34 : Quel protocole attribue automatiquement une adresse IP ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (107, 34, 'DHCP', TRUE,  1.00),
    (108, 34, 'DNS',  FALSE, 0.00),
    (109, 34, 'ICMP', FALSE, 0.00),
    (110, 34, 'SNMP', FALSE, 0.00);

-- Q35 : Quels protocoles operent au niveau de la couche transport ?
INSERT INTO answers (id, question_id, label, is_correct, weight) VALUES
    (111, 35, 'TCP',  TRUE,  1.00),
    (112, 35, 'UDP',  TRUE,  1.00),
    (113, 35, 'HTTP', FALSE, 0.00),
    (114, 35, 'IP',   FALSE, 0.00);


-- ============================================================
-- QUIZ SUBMISSIONS
-- max_score : somme des weights positifs de toutes les questions
--   Q1 (5 questions x1)    = 5.00
--   Q2 (5 questions x1)    = 5.00
--   Q3 (5 questions x1)    = 5.00
--   Q4 (4q x1 + Q19 x3)   = 7.00
--   Q5 (4 questions x1)    = 4.00
--   Q8 (3q x1 + Q35 x2)   = 5.00
-- ============================================================

INSERT INTO quiz_submissions (id, user_id, questionnaire_id, score, max_score, created_at) VALUES
    (1,  2, 1, 5.00, 5.00, '2026-03-01 10:30:00'),  -- Jean  : Q1 100%
    (2,  3, 1, 2.00, 5.00, '2026-03-05 14:15:00'),  -- Marie : Q1  40%
    (3,  3, 2, 5.00, 5.00, '2026-03-08 09:00:00'),  -- Marie : Q2 100%
    (4,  4, 3, 4.00, 5.00, '2026-03-10 16:00:00'),  -- Thomas: Q3  80%
    (5,  2, 4, 7.00, 7.00, '2026-03-12 11:45:00'),  -- Jean  : Q4 100%
    (6,  4, 1, 3.00, 5.00, '2026-03-15 15:30:00'),  -- Thomas: Q1  60%
    (7,  2, 8, 5.00, 5.00, '2026-03-18 10:00:00'),  -- Jean  : Q8 100%
    (8,  3, 5, 2.00, 4.00, '2026-03-20 13:30:00'),  -- Marie : Q5  50%
    (9,  4, 4, 3.00, 7.00, '2026-03-22 17:00:00'),  -- Thomas: Q4  43%
    (10, 1, 2, 4.00, 5.00, '2026-04-01 09:30:00');  -- Alice : Q2  80%


-- ============================================================
-- QUIZ ANSWERS
-- ============================================================

-- Submission 1 : Jean, Q1 Les bases de HTML et CSS (5/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (1,  1, 1,  2),   -- Q1  -> Faux (correct)
    (2,  1, 2,  3),   -- Q2  -> <a> (correct)
    (3,  1, 3,  7),   -- Q3  -> color (correct)
    (4,  1, 4,  11),  -- Q4  -> Vrai (correct)
    (5,  1, 5,  13);  -- Q5  -> <nav> (correct)

-- Submission 2 : Marie, Q1 Les bases de HTML et CSS (2/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (6,  2, 1,  1),   -- Q1  -> Vrai (faux)
    (7,  2, 2,  3),   -- Q2  -> <a> (correct)
    (8,  2, 3,  8),   -- Q3  -> font-color (faux)
    (9,  2, 4,  11),  -- Q4  -> Vrai (correct)
    (10, 2, 5,  14);  -- Q5  -> <navigation> (faux)

-- Submission 3 : Marie, Q2 Fondamentaux des reseaux (5/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (11, 3, 6,  17),  -- Q6  -> SMTP (correct)
    (12, 3, 7,  22),  -- Q7  -> Faux (correct)
    (13, 3, 8,  23),  -- Q8  -> 443 (correct)
    (14, 3, 9,  27),  -- Q9  -> Couche 3 (correct)
    (15, 3, 10, 31);  -- Q10 -> Vrai (correct)

-- Submission 4 : Thomas, Q3 Securite informatique 101 (4/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (16, 4, 11, 33),  -- Q11 -> Phishing (correct)
    (17, 4, 12, 38),  -- Q12 -> Faux (correct)
    (18, 4, 13, 39),  -- Q13 -> HyperText Transfer Protocol Secure (correct)
    (19, 4, 14, 43),  -- Q14 -> Vrai (correct)
    (20, 4, 15, 47);  -- Q15 -> 6 caracteres (faux)

-- Submission 5 : Jean, Q4 SQL et bases de donnees (7/7)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (21, 5, 16, 49),  -- Q16 -> SELECT (correct)
    (22, 5, 17, 54),  -- Q17 -> Faux (correct)
    (23, 5, 18, 55),  -- Q18 -> INNER JOIN (correct)
    (24, 5, 19, 59),  -- Q19 -> Atomicite (correct)
    (25, 5, 19, 60),  -- Q19 -> Coherence (correct)
    (26, 5, 19, 62),  -- Q19 -> Durabilite (correct)
    (27, 5, 20, 63);  -- Q20 -> Vrai (correct)

-- Submission 6 : Thomas, Q1 Les bases de HTML et CSS (3/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (28, 6, 1,  2),   -- Q1  -> Faux (correct)
    (29, 6, 2,  4),   -- Q2  -> <link> (faux)
    (30, 6, 3,  7),   -- Q3  -> color (correct)
    (31, 6, 4,  12),  -- Q4  -> Faux (faux)
    (32, 6, 5,  13);  -- Q5  -> <nav> (correct)

-- Submission 7 : Jean, Q8 Protocoles reseau avances (5/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (33, 7, 32, 101), -- Q32 -> DNS (correct)
    (34, 7, 33, 106), -- Q33 -> Faux (correct)
    (35, 7, 34, 107), -- Q34 -> DHCP (correct)
    (36, 7, 35, 111), -- Q35 -> TCP (correct)
    (37, 7, 35, 112); -- Q35 -> UDP (correct)

-- Submission 8 : Marie, Q5 Culture generale IT (2/4)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (38, 8, 21, 65),  -- Q21 -> Alan Turing (correct)
    (39, 8, 22, 69),  -- Q22 -> Vrai (faux)
    (40, 8, 23, 72),  -- Q23 -> 1995 (faux)
    (41, 8, 24, 75);  -- Q24 -> Swift (correct)

-- Submission 9 : Thomas, Q4 SQL et bases de donnees (3/7)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (42, 9, 16, 49),  -- Q16 -> SELECT (correct)
    (43, 9, 17, 53),  -- Q17 -> Vrai (faux)
    (44, 9, 18, 56),  -- Q18 -> LEFT JOIN (faux)
    (45, 9, 19, 59),  -- Q19 -> Atomicite (correct, partiel)
    (46, 9, 19, 61),  -- Q19 -> Rapidite (faux)
    (47, 9, 20, 63);  -- Q20 -> Vrai (correct)

-- Submission 10 : Alice, Q2 Fondamentaux des reseaux (4/5)
INSERT INTO quiz_answers (id, quiz_submission_id, question_id, answer_id) VALUES
    (48, 10, 6,  17),  -- Q6  -> SMTP (correct)
    (49, 10, 7,  22),  -- Q7  -> Faux (correct)
    (50, 10, 8,  24),  -- Q8  -> 80 (faux)
    (51, 10, 9,  27),  -- Q9  -> Couche 3 (correct)
    (52, 10, 10, 31);  -- Q10 -> Vrai (correct)


-- ============================================================
-- QUESTION FEEDBACKS
-- ============================================================

INSERT INTO question_feedbacks (id, user_id, question_id, rating, comment, created_at) VALUES
    (1, 2, 1,  4, 'Question claire et bien formulee.',              '2026-03-01 10:35:00'),
    (2, 3, 6,  5, 'Excellente question sur les protocoles !',       '2026-03-08 09:10:00'),
    (3, 4, 11, 3, NULL,                                              '2026-03-10 16:10:00'),
    (4, 2, 16, 5, 'Tres pertinent pour reviser le SQL.',            '2026-03-12 11:50:00'),
    (5, 3, 21, 4, NULL,                                              '2026-03-20 13:35:00'),
    (6, 4, 2,  2, 'La question pourrait etre plus precise.',        '2026-03-15 15:35:00'),
    (7, 2, 32, 5, 'Parfait pour reviser les protocoles.',           '2026-03-18 10:05:00'),
    (8, 1, 7,  4, 'Bonne question piege sur IPv4 vs IPv6.',         '2026-04-01 09:35:00');


-- ============================================================
-- ACTIVITY LOGS
-- ============================================================

INSERT INTO activity_logs (id, user_id, action, entity_type, entity_id, details, source, created_at) VALUES
    (1,  1, 'register',               'user',          1,  NULL,                              'web',     '2026-01-10 09:00:00'),
    (2,  1, 'login',                  'user',          1,  NULL,                              'web',     '2026-01-10 09:01:00'),
    (3,  2, 'register',               'user',          2,  NULL,                              'web',     '2026-01-15 14:30:00'),
    (4,  3, 'register',               'user',          3,  NULL,                              'web',     '2026-02-01 10:00:00'),
    (5,  4, 'register',               'user',          4,  NULL,                              'web',     '2026-02-20 16:45:00'),
    (6,  1, 'questionnaire.create',   'questionnaire', 1,  'Les bases de HTML et CSS',        'web',     '2026-02-10 10:00:00'),
    (7,  1, 'questionnaire.publish',  'questionnaire', 1,  'Les bases de HTML et CSS',        'web',     '2026-02-10 10:30:00'),
    (8,  2, 'questionnaire.create',   'questionnaire', 2,  'Fondamentaux des reseaux',        'desktop', '2026-02-15 09:00:00'),
    (9,  2, 'questionnaire.publish',  'questionnaire', 2,  'Fondamentaux des reseaux',        'desktop', '2026-02-15 09:30:00'),
    (10, 1, 'questionnaire.create',   'questionnaire', 3,  'Securite informatique 101',       'web',     '2026-02-18 14:00:00'),
    (11, 1, 'questionnaire.publish',  'questionnaire', 3,  'Securite informatique 101',       'web',     '2026-02-18 14:30:00'),
    (12, 3, 'questionnaire.create',   'questionnaire', 4,  'SQL et bases de donnees',         'web',     '2026-02-25 11:00:00'),
    (13, 3, 'questionnaire.publish',  'questionnaire', 4,  'SQL et bases de donnees',         'desktop', '2026-02-25 11:30:00'),
    (14, 2, 'questionnaire.create',   'questionnaire', 5,  'Culture generale IT',             'desktop', '2026-02-28 16:00:00'),
    (15, 2, 'questionnaire.publish',  'questionnaire', 5,  'Culture generale IT',             'web',     '2026-02-28 16:30:00'),
    (16, 4, 'questionnaire.create',   'questionnaire', 6,  'Introduction a Docker',           'desktop', '2026-03-05 10:00:00'),
    (17, 3, 'questionnaire.create',   'questionnaire', 7,  'JavaScript avance',               'web',     '2026-03-06 14:00:00'),
    (18, 1, 'questionnaire.create',   'questionnaire', 8,  'Protocoles reseau avances',       'web',     '2026-03-07 09:00:00'),
    (19, 1, 'questionnaire.publish',  'questionnaire', 8,  'Protocoles reseau avances',       'web',     '2026-03-07 09:30:00'),
    (20, 2, 'login',                  'user',          2,  NULL,                              'web',     '2026-03-01 10:25:00'),
    (21, 2, 'quiz.complete',          'questionnaire', 1,  'Les bases de HTML et CSS - 5/5',  'web',     '2026-03-01 10:30:00'),
    (22, 3, 'login',                  'user',          3,  NULL,                              'desktop', '2026-03-05 14:10:00'),
    (23, 3, 'quiz.complete',          'questionnaire', 1,  'Les bases de HTML et CSS - 2/5',  'desktop', '2026-03-05 14:15:00'),
    (24, 3, 'quiz.complete',          'questionnaire', 2,  'Fondamentaux des reseaux - 5/5',  'web',     '2026-03-08 09:00:00'),
    (25, 4, 'login',                  'user',          4,  NULL,                              'desktop', '2026-03-10 15:55:00'),
    (26, 4, 'quiz.complete',          'questionnaire', 3,  'Securite informatique 101 - 4/5', 'desktop', '2026-03-10 16:00:00'),
    (27, 2, 'quiz.complete',          'questionnaire', 4,  'SQL et bases de donnees - 7/7',   'web',     '2026-03-12 11:45:00'),
    (28, 4, 'quiz.complete',          'questionnaire', 1,  'Les bases de HTML et CSS - 3/5',  'web',     '2026-03-15 15:30:00'),
    (29, 2, 'quiz.complete',          'questionnaire', 8,  'Protocoles reseau avances - 5/5', 'desktop', '2026-03-18 10:00:00'),
    (30, 3, 'quiz.complete',          'questionnaire', 5,  'Culture generale IT - 2/4',       'web',     '2026-03-20 13:30:00'),
    (31, 4, 'quiz.complete',          'questionnaire', 4,  'SQL et bases de donnees - 3/7',   'desktop', '2026-03-22 17:00:00'),
    (32, 1, 'login',                  'user',          1,  NULL,                              'web',     '2026-04-01 09:25:00'),
    (33, 1, 'quiz.complete',          'questionnaire', 2,  'Fondamentaux des reseaux - 4/5',  'web',     '2026-04-01 09:30:00'),
    (34, 1, 'questionnaire.update',   'questionnaire', 3,  'Securite informatique 101',       'web',     '2026-04-05 11:00:00'),
    (35, 2, 'login',                  'user',          2,  NULL,                              'desktop', '2026-04-10 08:00:00'),
    (36, 3, 'login',                  'user',          3,  NULL,                              'web',     '2026-04-12 10:00:00'),
    (37, 4, 'questionnaire.update',   'questionnaire', 6,  'Introduction a Docker',           'desktop', '2026-04-15 14:00:00'),
    (38, 1, 'login',                  'user',          1,  NULL,                              'desktop', '2026-04-18 09:00:00');


-- ============================================================
-- SEQUENCE RESETS
-- ============================================================

SELECT setval('themes_id_seq',             6);
SELECT setval('users_id_seq',              5);
SELECT setval('questionnaires_id_seq',     8);
SELECT setval('questions_id_seq',          35);
SELECT setval('answers_id_seq',            114);
SELECT setval('quiz_submissions_id_seq',   10);
SELECT setval('quiz_answers_id_seq',       52);
SELECT setval('question_feedbacks_id_seq', 8);
SELECT setval('activity_logs_id_seq',      38);
