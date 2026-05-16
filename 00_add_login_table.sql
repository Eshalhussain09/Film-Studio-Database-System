-- ============================================================
-- RUN THIS AFTER film_studio_complete.sql
-- Adds the app_user login table for all 15+15+15 users
-- ============================================================

-- Drop if exists
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE app_user CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE app_user (
  user_id     NUMBER PRIMARY KEY,
  party_id    NUMBER NOT NULL,          -- links to actor_id / director_id / crew_id
  role        VARCHAR2(20) NOT NULL CHECK (role IN ('Admin','Director','Actor','Crew')),
  username    VARCHAR2(60) NOT NULL UNIQUE,
  password    VARCHAR2(60) NOT NULL,
  full_name   VARCHAR2(100)
);

-- ---- ADMIN (1 super admin) -----------------------------------------
INSERT INTO app_user VALUES (100, 0, 'Admin', 'admin', 'admin123', 'Studio Admin');

-- ---- DIRECTORS (party_id = director_id) ----------------------------
INSERT INTO app_user VALUES (1,  1,  'Director', 'shoaib.mansoor',     'dir001', 'Shoaib Mansoor');
INSERT INTO app_user VALUES (2,  2,  'Director', 'asim.raza',          'dir002', 'Asim Raza');
INSERT INTO app_user VALUES (3,  3,  'Director', 'nabeel.qureshi',     'dir003', 'Nabeel Qureshi');
INSERT INTO app_user VALUES (4,  4,  'Director', 'mehreen.jabbar',     'dir004', 'Mehreen Jabbar');
INSERT INTO app_user VALUES (5,  5,  'Director', 'bilal.lashari',      'dir005', 'Bilal Lashari');
INSERT INTO app_user VALUES (6,  6,  'Director', 'sarmad.khoosat',     'dir006', 'Sarmad Khoosat');
INSERT INTO app_user VALUES (7,  7,  'Director', 'jami.moor',          'dir007', 'Jami Moor');
INSERT INTO app_user VALUES (8,  8,  'Director', 'haseeb.hassan',      'dir008', 'Haseeb Hassan');
INSERT INTO app_user VALUES (9,  9,  'Director', 'ali.zafar',          'dir009', 'Ali Abbas Zafar');
INSERT INTO app_user VALUES (10, 10, 'Director', 'momina.duraid',      'dir010', 'Momina Duraid');
INSERT INTO app_user VALUES (11, 11, 'Director', 'farhan.alam',        'dir011', 'Farhan Alam');
INSERT INTO app_user VALUES (12, 12, 'Director', 'wajahat.rauf',       'dir012', 'Wajahat Rauf');
INSERT INTO app_user VALUES (13, 13, 'Director', 'shahzad.kashmiri',   'dir013', 'Shahzad Kashmiri');
INSERT INTO app_user VALUES (14, 14, 'Director', 'adnan.sarwar',       'dir014', 'Adnan Sarwar');
INSERT INTO app_user VALUES (15, 15, 'Director', 'iram.bilal',         'dir015', 'Iram Parveen Bilal');

-- ---- ACTORS (party_id = actor_id, user_id offset 200) ---------------
INSERT INTO app_user VALUES (201, 1,  'Actor', 'fawad.khan',      'act001', 'Fawad Khan');
INSERT INTO app_user VALUES (202, 2,  'Actor', 'mahira.khan',     'act002', 'Mahira Khan');
INSERT INTO app_user VALUES (203, 3,  'Actor', 'hamza.abbasi',    'act003', 'Hamza Ali Abbasi');
INSERT INTO app_user VALUES (204, 4,  'Actor', 'sajal.aly',       'act004', 'Sajal Aly');
INSERT INTO app_user VALUES (205, 5,  'Actor', 'bilal.ashraf',    'act005', 'Bilal Ashraf');
INSERT INTO app_user VALUES (206, 6,  'Actor', 'hania.aamir',     'act006', 'Hania Aamir');
INSERT INTO app_user VALUES (207, 7,  'Actor', 'humayun.saeed',   'act007', 'Humayun Saeed');
INSERT INTO app_user VALUES (208, 8,  'Actor', 'ayeza.khan',      'act008', 'Ayeza Khan');
INSERT INTO app_user VALUES (209, 9,  'Actor', 'zahid.ahmed',     'act009', 'Zahid Ahmed');
INSERT INTO app_user VALUES (210, 10, 'Actor', 'urwa.hocane',     'act010', 'Urwa Hocane');
INSERT INTO app_user VALUES (211, 11, 'Actor', 'ali.rehman',      'act011', 'Ali Rehman Khan');
INSERT INTO app_user VALUES (212, 12, 'Actor', 'syra.shehroz',    'act012', 'Syra Shehroz');
INSERT INTO app_user VALUES (213, 13, 'Actor', 'yasir.hussain',   'act013', 'Yasir Hussain');
INSERT INTO app_user VALUES (214, 14, 'Actor', 'saba.qamar',      'act014', 'Saba Qamar');
INSERT INTO app_user VALUES (215, 15, 'Actor', 'imran.abbas',     'act015', 'Imran Abbas');

-- ---- CREW (party_id = crew_id, user_id offset 300) ------------------
INSERT INTO app_user VALUES (301, 1,  'Crew', 'rana.kamran',      'crew001', 'Rana Kamran');
INSERT INTO app_user VALUES (302, 2,  'Crew', 'zara.noor',        'crew002', 'Zara Noor Abbas');
INSERT INTO app_user VALUES (303, 3,  'Crew', 'usman.tahir',      'crew003', 'Usman Tahir');
INSERT INTO app_user VALUES (304, 4,  'Crew', 'farrukh.bashir',   'crew004', 'Farrukh Bashir');
INSERT INTO app_user VALUES (305, 5,  'Crew', 'nida.yasir',       'crew005', 'Nida Yasir');
INSERT INTO app_user VALUES (306, 6,  'Crew', 'kashif.nisar',     'crew006', 'Kashif Nisar');
INSERT INTO app_user VALUES (307, 7,  'Crew', 'misbah.khalid',    'crew007', 'Misbah Khalid');
INSERT INTO app_user VALUES (308, 8,  'Crew', 'tariq.mehmood',    'crew008', 'Tariq Mehmood');
INSERT INTO app_user VALUES (309, 9,  'Crew', 'amina.haq',        'crew009', 'Amina Haq');
INSERT INTO app_user VALUES (310, 10, 'Crew', 'shahzad.raza',     'crew010', 'Shahzad Raza');
INSERT INTO app_user VALUES (311, 11, 'Crew', 'rabia.zulfiqar',   'crew011', 'Rabia Zulfiqar');
INSERT INTO app_user VALUES (312, 12, 'Crew', 'hamid.latif',      'crew012', 'Hamid Latif');
INSERT INTO app_user VALUES (313, 13, 'Crew', 'sadia.imam',       'crew013', 'Sadia Imam');
INSERT INTO app_user VALUES (314, 14, 'Crew', 'naveed.raza',      'crew014', 'Naveed Raza');
INSERT INTO app_user VALUES (315, 15, 'Crew', 'umair.jaswal',     'crew015', 'Umair Jaswal');

COMMIT;
/