 -- ============================================================
-- FILM STUDIO DATABASE SYSTEM
-- Oracle 11g Compatible
-- Group Members: Eshal Hussain 24F-0597 | Ezza Bilal 24F-0603
-- ============================================================

-- ============================================================
-- SECTION 1: DROP EXISTING TABLES (Clean Start)
-- ============================================================

  EXECUTE IMMEDIATE 'DROP TABLE schedule CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE crew_assignment CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE scene CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE contract CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE cast_role CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE movie CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE director CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE actor CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE crew_member CASCADE CONSTRAINTS';
  EXECUTE IMMEDIATE 'DROP TABLE shooting_location CASCADE CONSTRAINTS';
EXCEPTION
  WHEN OTHERS THEN NULL;
END;
/

-- ============================================================
-- SECTION 2: CREATE TABLES
-- ============================================================

-- Director Table
CREATE TABLE director (
  director_id   NUMBER PRIMARY KEY,
  full_name     VARCHAR2(100) NOT NULL,
  dob           DATE,
  nationality   VARCHAR2(50),
  biography     VARCHAR2(500)
);

-- Movie Table
CREATE TABLE movie (
  movie_id      NUMBER PRIMARY KEY,
  title         VARCHAR2(150) NOT NULL,
  release_year  NUMBER(4),
  genre         VARCHAR2(50),
  rating        VARCHAR2(10),
  budget        NUMBER(15,2),
  status        VARCHAR2(30) CHECK (status IN ('Pre-Production','In Production','Post-Production','Released','Cancelled')),
  director_id   NUMBER,
  CONSTRAINT fk_movie_director FOREIGN KEY (director_id) REFERENCES director(director_id)
);

-- Actor Table
CREATE TABLE actor (
  actor_id      NUMBER PRIMARY KEY,
  full_name     VARCHAR2(100) NOT NULL,
  dob           DATE,
  nationality   VARCHAR2(50),
  contact       VARCHAR2(100)
);

-- Crew Member Table
CREATE TABLE crew_member (
  crew_id         NUMBER PRIMARY KEY,
  full_name       VARCHAR2(100) NOT NULL,
  department      VARCHAR2(50),
  specialization  VARCHAR2(100)
);

-- Shooting Location Table
CREATE TABLE shooting_location (
  location_id   NUMBER PRIMARY KEY,
  name          VARCHAR2(100) NOT NULL,
  address       VARCHAR2(200),
  location_type VARCHAR2(50),
  country       VARCHAR2(50),
  daily_rate    NUMBER(10,2)
);

-- Cast Role Table
CREATE TABLE cast_role (
  role_id         NUMBER PRIMARY KEY,
  movie_id        NUMBER NOT NULL,
  actor_id        NUMBER NOT NULL,
  character_name  VARCHAR2(100),
  role_type       VARCHAR2(30) CHECK (role_type IN ('Lead','Supporting','Cameo','Narrator')),
  billing_order   NUMBER,
  CONSTRAINT fk_castrole_movie  FOREIGN KEY (movie_id)  REFERENCES movie(movie_id),
  CONSTRAINT fk_castrole_actor  FOREIGN KEY (actor_id)  REFERENCES actor(actor_id)
);

-- Contract Table
CREATE TABLE contract (
  contract_id   NUMBER PRIMARY KEY,
  movie_id      NUMBER NOT NULL,
  party_id      NUMBER,
  party_type    VARCHAR2(30) CHECK (party_type IN ('Actor','Crew','Director','Vendor')),
  signed_date   DATE,
  expiry_date   DATE,
  total_value   NUMBER(15,2),
  terms_summary VARCHAR2(500),
  CONSTRAINT fk_contract_movie FOREIGN KEY (movie_id) REFERENCES movie(movie_id)
);

-- Scene Table
CREATE TABLE scene (
  scene_id      NUMBER PRIMARY KEY,
  movie_id      NUMBER NOT NULL,
  location_id   NUMBER,
  scene_no      NUMBER NOT NULL,
  description   VARCHAR2(300),
  duration      NUMBER(5,2),
  status        VARCHAR2(30) CHECK (status IN ('Planned','Shooting','Completed','Cancelled')),
  CONSTRAINT fk_scene_movie    FOREIGN KEY (movie_id)    REFERENCES movie(movie_id),
  CONSTRAINT fk_scene_location FOREIGN KEY (location_id) REFERENCES shooting_location(location_id)
);

-- Crew Assignment Table
CREATE TABLE crew_assignment (
  assignment_id NUMBER PRIMARY KEY,
  movie_id      NUMBER NOT NULL,
  crew_id       NUMBER NOT NULL,
  job_title     VARCHAR2(100),
  start_date    DATE,
  end_date      DATE,
  CONSTRAINT fk_assignment_movie FOREIGN KEY (movie_id) REFERENCES movie(movie_id),
  CONSTRAINT fk_assignment_crew  FOREIGN KEY (crew_id)  REFERENCES crew_member(crew_id)
);

-- Schedule Table
CREATE TABLE schedule (
  schedule_id   NUMBER PRIMARY KEY,
  movie_id      NUMBER NOT NULL,
  scene_id      NUMBER NOT NULL,
  location_id   NUMBER,
  shoot_date    DATE,
  start_time    DATE,
  end_time      DATE,
  status        VARCHAR2(30) CHECK (status IN ('Scheduled','In Progress','Completed','Postponed')),
  CONSTRAINT fk_schedule_movie    FOREIGN KEY (movie_id)    REFERENCES movie(movie_id),
  CONSTRAINT fk_schedule_scene    FOREIGN KEY (scene_id)    REFERENCES scene(scene_id),
  CONSTRAINT fk_schedule_location FOREIGN KEY (location_id) REFERENCES shooting_location(location_id)
);

-- ============================================================
-- SECTION 3: INSERT SAMPLE DATA
-- ============================================================

-- ============================================================
-- DIRECTORS (15 rows)
-- ============================================================
INSERT ALL
  INTO director VALUES (1,  'Shoaib Mansoor',    TO_DATE('15-03-1959','DD-MM-YYYY'), 'Pakistani', 'Legendary director of Khuda Kay Liye, Bol, and Verna. Pioneer of Pakistani cinema revival.')
  INTO director VALUES (2,  'Asim Raza',         TO_DATE('22-07-1967','DD-MM-YYYY'), 'Pakistani', 'Known for Ho Mann Jahaan, Parey Hut Love. Also a celebrated music video director.')
  INTO director VALUES (3,  'Nabeel Qureshi',    TO_DATE('10-11-1980','DD-MM-YYYY'), 'Pakistani', 'Director of Na Maloom Afraad franchise and Load Wedding. Comedy specialist.')
  INTO director VALUES (4,  'Mehreen Jabbar',    TO_DATE('05-09-1970','DD-MM-YYYY'), 'Pakistani', 'Directed Ramchand Pakistani and Dobara Phir Se. Prominent female director.')
  INTO director VALUES (5,  'Bilal Lashari',     TO_DATE('18-04-1982','DD-MM-YYYY'), 'Pakistani', 'Director of Waar and The Legend of Maula Jatt — Pakistans highest-grossing film.')
  INTO director VALUES (6,  'Sarmad Khoosat',    TO_DATE('30-06-1978','DD-MM-YYYY'), 'Pakistani', 'Director of Manto and Zindagi Tamasha. Known for bold and artistic projects.')
  INTO director VALUES (7,  'Jami Moor',         TO_DATE('12-02-1975','DD-MM-YYYY'), 'Pakistani', 'Directed 007 Shab and Moor. Known for cinematic visuals and social storytelling.')
  INTO director VALUES (8,  'Haseeb Hassan',     TO_DATE('25-08-1985','DD-MM-YYYY'), 'Pakistani', 'Popular drama and film director known for Dillagi and Koi Chand Rakh.')
  INTO director VALUES (9,  'Ali Abbas Zafar',   TO_DATE('17-05-1983','DD-MM-YYYY'), 'Pakistani', 'Cross-border director known for Sultan and Bharat in Bollywood; now returning to Pakistan.')
  INTO director VALUES (10, 'Momina Duraid',     TO_DATE('03-01-1973','DD-MM-YYYY'), 'Pakistani', 'HUM TV executive and producer-director behind blockbuster dramas like Humsafar.')
  INTO director VALUES (11, 'Farhan Alam',       TO_DATE('14-06-1984','DD-MM-YYYY'), 'Pakistani', 'Director of Cake and drama serial Mann Mayal. Known for slice-of-life storytelling.')
  INTO director VALUES (12, 'Wajahat Rauf',      TO_DATE('09-12-1979','DD-MM-YYYY'), 'Pakistani', 'Director of Karachi Se Lahore franchise. Specializes in road-trip comedies.')
  INTO director VALUES (13, 'Shahzad Kashmiri',  TO_DATE('20-10-1976','DD-MM-YYYY'), 'Pakistani', 'Directed Jawani Phir Nahi Aani series. Blockbuster comedy filmmaker.')
  INTO director VALUES (14, 'Adnan Sarwar',      TO_DATE('07-07-1980','DD-MM-YYYY'), 'Pakistani', 'Actor-director known for Shah (2015) — first Pakistani war film.')
  INTO director VALUES (15, 'Iram Parveen Bilal',TO_DATE('29-03-1978','DD-MM-YYYY'), 'Pakistani', 'Directed Josh and Ill Met by Moonlight. Feminist filmmaker and screenwriter.')
SELECT * FROM dual;

-- ============================================================
-- MOVIES (15 rows)
-- ============================================================
INSERT ALL
  INTO movie VALUES (1,  'Lahore 1947',           2025, 'Historical', 'U/A', 450000000, 'In Production',    5)
  INTO movie VALUES (2,  'Dil Bechara Yaar',      2025, 'Romance',    'U',   120000000, 'Pre-Production',   2)
  INTO movie VALUES (3,  'Khamoshi',              2024, 'Thriller',   'A',   180000000, 'Post-Production',  6)
  INTO movie VALUES (4,  'Na Maloom Afraad 3',    2025, 'Comedy',     'U',   200000000, 'In Production',    3)
  INTO movie VALUES (5,  'Janaan 2',              2026, 'Romance',    'U',   150000000, 'Pre-Production',   4)
  INTO movie VALUES (6,  'Quaid Ka Pakistan',     2024, 'Biography',  'U/A', 300000000, 'Released',         1)
  INTO movie VALUES (7,  'Superstar 2',           2025, 'Musical',    'U',   170000000, 'In Production',    2)
  INTO movie VALUES (8,  'Wajood',                2025, 'Drama',      'U/A', 130000000, 'Pre-Production',   7)
  INTO movie VALUES (9,  'Karachi Blues',         2026, 'Crime',      'A',   220000000, 'Pre-Production',  12)
  INTO movie VALUES (10, 'Mohabbat Zindagi Hai',  2024, 'Romance',    'U',    90000000, 'Released',         8)
  INTO movie VALUES (11, 'Saaya',                 2025, 'Horror',     'A',   160000000, 'In Production',   11)
  INTO movie VALUES (12, 'Badshah Khan',          2026, 'Action',     'U/A', 500000000, 'Pre-Production',   5)
  INTO movie VALUES (13, 'Teri Meri Kahani 2',    2025, 'Romance',    'U',   110000000, 'In Production',   13)
  INTO movie VALUES (14, 'Zulm Ki Raat',          2025, 'Thriller',   'A',   140000000, 'Post-Production', 15)
  INTO movie VALUES (15, 'Roshni',                2024, 'Drama',      'U',    80000000, 'Released',        10)
SELECT * FROM dual;

-- ============================================================
-- ACTORS (15 rows)
-- ============================================================
INSERT ALL
  INTO actor VALUES (1,  'Fawad Khan',        TO_DATE('29-11-1981','DD-MM-YYYY'), 'Pakistani', 'fawad.khan@humfilms.pk')
  INTO actor VALUES (2,  'Mahira Khan',       TO_DATE('21-12-1984','DD-MM-YYYY'), 'Pakistani', 'mahira.khan@humfilms.pk')
  INTO actor VALUES (3,  'Hamza Ali Abbasi',  TO_DATE('23-06-1984','DD-MM-YYYY'), 'Pakistani', 'hamza.abbasi@humfilms.pk')
  INTO actor VALUES (4,  'Sajal Aly',         TO_DATE('17-01-1994','DD-MM-YYYY'), 'Pakistani', 'sajal.aly@humfilms.pk')
  INTO actor VALUES (5,  'Bilal Ashraf',      TO_DATE('08-08-1988','DD-MM-YYYY'), 'Pakistani', 'bilal.ashraf@humfilms.pk')
  INTO actor VALUES (6,  'Hania Aamir',       TO_DATE('12-02-1997','DD-MM-YYYY'), 'Pakistani', 'hania.aamir@humfilms.pk')
  INTO actor VALUES (7,  'Humayun Saeed',     TO_DATE('27-07-1971','DD-MM-YYYY'), 'Pakistani', 'humayun.saeed@humfilms.pk')
  INTO actor VALUES (8,  'Ayeza Khan',        TO_DATE('15-01-1991','DD-MM-YYYY'), 'Pakistani', 'ayeza.khan@humfilms.pk')
  INTO actor VALUES (9,  'Zahid Ahmed',       TO_DATE('04-03-1980','DD-MM-YYYY'), 'Pakistani', 'zahid.ahmed@humfilms.pk')
  INTO actor VALUES (10, 'Urwa Hocane',       TO_DATE('20-05-1990','DD-MM-YYYY'), 'Pakistani', 'urwa.hocane@humfilms.pk')
  INTO actor VALUES (11, 'Ali Rehman Khan',   TO_DATE('16-09-1986','DD-MM-YYYY'), 'Pakistani', 'ali.rehman@humfilms.pk')
  INTO actor VALUES (12, 'Syra Shehroz',      TO_DATE('24-07-1988','DD-MM-YYYY'), 'Pakistani', 'syra.shehroz@humfilms.pk')
  INTO actor VALUES (13, 'Yasir Hussain',     TO_DATE('22-04-1984','DD-MM-YYYY'), 'Pakistani', 'yasir.hussain@humfilms.pk')
  INTO actor VALUES (14, 'Saba Qamar',        TO_DATE('05-04-1984','DD-MM-YYYY'), 'Pakistani', 'saba.qamar@humfilms.pk')
  INTO actor VALUES (15, 'Imran Abbas',       TO_DATE('18-10-1979','DD-MM-YYYY'), 'Pakistani', 'imran.abbas@humfilms.pk')
SELECT * FROM dual;

-- ============================================================
-- CREW MEMBERS (15 rows)
-- ============================================================
INSERT ALL
  INTO crew_member VALUES (1,  'Rana Kamran',       'Cinematography', 'Director of Photography')
  INTO crew_member VALUES (2,  'Zara Noor Abbas',   'Production',     'Production Manager')
  INTO crew_member VALUES (3,  'Usman Tahir',       'Sound',          'Sound Engineer')
  INTO crew_member VALUES (4,  'Farrukh Bashir',    'Visual Effects', 'VFX Supervisor')
  INTO crew_member VALUES (5,  'Nida Yasir',        'Art Direction',  'Production Designer')
  INTO crew_member VALUES (6,  'Kashif Nisar',      'Editing',        'Film Editor')
  INTO crew_member VALUES (7,  'Misbah Khalid',     'Costume',        'Costume Designer')
  INTO crew_member VALUES (8,  'Tariq Mehmood',     'Stunts',         'Stunt Coordinator')
  INTO crew_member VALUES (9,  'Amina Haq',         'Makeup',         'Head Makeup Artist')
  INTO crew_member VALUES (10, 'Shahzad Raza',      'Lighting',       'Gaffer')
  INTO crew_member VALUES (11, 'Rabia Zulfiqar',    'Script',         'Script Supervisor')
  INTO crew_member VALUES (12, 'Hamid Latif',       'Cinematography', 'Camera Operator')
  INTO crew_member VALUES (13, 'Sadia Imam',        'Production',     'Line Producer')
  INTO crew_member VALUES (14, 'Naveed Raza',       'Sound',          'Boom Operator')
  INTO crew_member VALUES (15, 'Umair Jaswal',      'Music',          'Music Composer')
SELECT * FROM dual;

-- ============================================================
-- SHOOTING LOCATIONS (15 rows)
-- ============================================================
INSERT ALL
  INTO shooting_location VALUES (1,  'Evernew Studios Lahore',     'Main Boulevard, Gulberg III, Lahore',       'Indoor',  'Pakistan', 85000)
  INTO shooting_location VALUES (2,  'Karachi Beach Front',        'Sea View, Clifton, Karachi',                'Outdoor', 'Pakistan', 60000)
  INTO shooting_location VALUES (3,  'Lahore Fort Complex',        'Fort Road, Walled City, Lahore',            'Outdoor', 'Pakistan', 70000)
  INTO shooting_location VALUES (4,  'Islamabad Margalla Hills',   'Trail 3, Margalla Hills, Islamabad',        'Outdoor', 'Pakistan', 45000)
  INTO shooting_location VALUES (5,  'Peshawar Old City Bazaar',   'Qissa Khwani Bazaar, Peshawar',             'Outdoor', 'Pakistan', 40000)
  INTO shooting_location VALUES (6,  'HUM TV Production Studio',   'Korangi Industrial Area, Karachi',          'Indoor',  'Pakistan', 95000)
  INTO shooting_location VALUES (7,  'Hunza Valley Karimabad',     'Main Street, Karimabad, Hunza, Gilgit',     'Outdoor', 'Pakistan', 55000)
  INTO shooting_location VALUES (8,  'Badshahi Mosque Exterior',   'Hazuri Bagh, Lahore',                       'Outdoor', 'Pakistan', 75000)
  INTO shooting_location VALUES (9,  'Mohatta Palace Karachi',     'Clifton Block 4, Karachi',                  'Outdoor', 'Pakistan', 65000)
  INTO shooting_location VALUES (10, 'Shalimar Garden Lahore',     'GT Road, Baghbanpura, Lahore',              'Outdoor', 'Pakistan', 50000)
  INTO shooting_location VALUES (11, 'ARY Film Studio Karachi',    'ARY Complex, Tipu Sultan Road, Karachi',    'Indoor',  'Pakistan', 90000)
  INTO shooting_location VALUES (12, 'Swat Valley Mingora',        'Mingora City, Swat, KPK',                   'Outdoor', 'Pakistan', 48000)
  INTO shooting_location VALUES (13, 'Faisal Mosque Islamabad',    'Shah Faisal Ave, Islamabad',                'Outdoor', 'Pakistan', 42000)
  INTO shooting_location VALUES (14, 'Thar Desert Mithi',          'Mithi, Tharparkar, Sindh',                  'Outdoor', 'Pakistan', 35000)
  INTO shooting_location VALUES (15, 'Nathia Gali Hill Station',   'Nathia Gali, Abbottabad, KPK',              'Outdoor', 'Pakistan', 38000)
SELECT * FROM dual;

-- ============================================================
-- CAST ROLES (15 rows)
-- ============================================================
INSERT ALL
  INTO cast_role VALUES (1,  1,  1,  'Ranjit Singh Mirza',     'Lead',       1)
  INTO cast_role VALUES (2,  1,  2,  'Aisha Bibi',             'Lead',       2)
  INTO cast_role VALUES (3,  2,  5,  'Ahad Mirza',             'Lead',       1)
  INTO cast_role VALUES (4,  2,  6,  'Zoya Hussain',           'Lead',       2)
  INTO cast_role VALUES (5,  3,  9,  'Inspector Karrar',       'Lead',       1)
  INTO cast_role VALUES (6,  3,  14, 'Sara Gilani',            'Supporting', 2)
  INTO cast_role VALUES (7,  4,  13, 'Shakeel Chaudhry',       'Lead',       1)
  INTO cast_role VALUES (8,  4,  11, 'Rocky Bhai',             'Supporting', 2)
  INTO cast_role VALUES (9,  5,  4,  'Zara Khan',              'Lead',       1)
  INTO cast_role VALUES (10, 6,  7,  'Muhammad Ali Jinnah',    'Lead',       1)
  INTO cast_role VALUES (11, 7,  1,  'Sameer Shah',            'Lead',       1)
  INTO cast_role VALUES (12, 7,  2,  'Noor Fatima',            'Supporting', 2)
  INTO cast_role VALUES (13, 8,  3,  'Bilal Haider',           'Lead',       1)
  INTO cast_role VALUES (14, 9,  9,  'DIG Asad Mir',           'Lead',       1)
  INTO cast_role VALUES (15, 10, 8,  'Meera Javed',            'Lead',       1)
SELECT * FROM dual;

-- ============================================================
-- CONTRACTS (15 rows)
-- ============================================================
INSERT ALL
  INTO contract VALUES (1,  1,  1,  'Actor',    TO_DATE('01-01-2025','DD-MM-YYYY'), TO_DATE('31-12-2025','DD-MM-YYYY'), 35000000, 'Lead role contract with 75 shoot days and media rights.')
  INTO contract VALUES (2,  1,  2,  'Actor',    TO_DATE('01-01-2025','DD-MM-YYYY'), TO_DATE('31-12-2025','DD-MM-YYYY'), 28000000, 'Lead female role, 70 shoot days, brand endorsement clause.')
  INTO contract VALUES (3,  1,  5,  'Director', TO_DATE('15-12-2024','DD-MM-YYYY'), TO_DATE('30-06-2026','DD-MM-YYYY'), 20000000, 'Full directorial contract with post-production supervision.')
  INTO contract VALUES (4,  2,  5,  'Actor',    TO_DATE('01-02-2025','DD-MM-YYYY'), TO_DATE('30-09-2025','DD-MM-YYYY'), 15000000, 'Lead role, 50 shoot days, international promotions.')
  INTO contract VALUES (5,  2,  6,  'Actor',    TO_DATE('01-02-2025','DD-MM-YYYY'), TO_DATE('30-09-2025','DD-MM-YYYY'), 12000000, 'Female lead, 50 shoot days.')
  INTO contract VALUES (6,  3,  9,  'Actor',    TO_DATE('10-03-2024','DD-MM-YYYY'), TO_DATE('31-12-2024','DD-MM-YYYY'), 18000000, 'Lead role thriller, 60 days shoot, confidentiality clause.')
  INTO contract VALUES (7,  4,  13, 'Actor',    TO_DATE('01-03-2025','DD-MM-YYYY'), TO_DATE('31-08-2025','DD-MM-YYYY'), 10000000, 'Lead comedy role, 45 shoot days.')
  INTO contract VALUES (8,  5,  4,  'Actor',    TO_DATE('01-06-2025','DD-MM-YYYY'), TO_DATE('28-02-2026','DD-MM-YYYY'), 14000000, 'Lead role in sequel, full production period.')
  INTO contract VALUES (9,  6,  7,  'Actor',    TO_DATE('01-01-2024','DD-MM-YYYY'), TO_DATE('31-12-2024','DD-MM-YYYY'), 25000000, 'Biographical lead role, 90 shoot days, historical research assistance.')
  INTO contract VALUES (10, 7,  1,  'Actor',    TO_DATE('01-04-2025','DD-MM-YYYY'), TO_DATE('31-12-2025','DD-MM-YYYY'), 22000000, 'Musical lead, includes song recording sessions.')
  INTO contract VALUES (11, 8,  3,  'Actor',    TO_DATE('01-05-2025','DD-MM-YYYY'), TO_DATE('28-02-2026','DD-MM-YYYY'), 16000000, 'Drama lead, 55 shoot days.')
  INTO contract VALUES (12, 9,  9,  'Actor',    TO_DATE('01-07-2025','DD-MM-YYYY'), TO_DATE('31-03-2026','DD-MM-YYYY'), 19000000, 'Crime drama lead role, night shoot allowance included.')
  INTO contract VALUES (13, 11, 4,  'Actor',    TO_DATE('01-04-2025','DD-MM-YYYY'), TO_DATE('31-10-2025','DD-MM-YYYY'),  9000000, 'Horror supporting lead, 35 shoot days.')
  INTO contract VALUES (14, 12, 1,  'Actor',    TO_DATE('01-01-2026','DD-MM-YYYY'), TO_DATE('31-12-2026','DD-MM-YYYY'), 40000000, 'Action lead, stunt training included, international release rights.')
  INTO contract VALUES (15, 13, 8,  'Actor',    TO_DATE('01-03-2025','DD-MM-YYYY'), TO_DATE('30-09-2025','DD-MM-YYYY'), 11000000, 'Romantic lead, 48 shoot days.')
SELECT * FROM dual;

-- ============================================================
-- SCENES (20 rows)
-- ============================================================
INSERT ALL
  INTO scene VALUES (1,  1,  3,  1,  'Partition night — families flee Lahore amid riots.',           18.0, 'Shooting')
  INTO scene VALUES (2,  1,  8,  2,  'Ranjit bids farewell at the Badshahi Mosque at dawn.',         10.5, 'Planned')
  INTO scene VALUES (3,  1,  5,  3,  'Aisha crosses the border — emotional goodbye scene.',          12.0, 'Planned')
  INTO scene VALUES (4,  2,  7,  1,  'Ahad proposes to Zoya in the Hunza Valley at sunset.',          6.5, 'Planned')
  INTO scene VALUES (5,  2,  4,  2,  'Family confrontation in the Margalla Hills bungalow.',          8.0, 'Planned')
  INTO scene VALUES (6,  3,  9,  1,  'Inspector discovers dead body at Mohatta Palace.',             14.0, 'Shooting')
  INTO scene VALUES (7,  3,  2,  2,  'Midnight chase along Karachi Beach Front.',                    16.0, 'Shooting')
  INTO scene VALUES (8,  4,  6,  1,  'Comedy skit at HUM TV Studio auditions.',                       5.0, 'Completed')
  INTO scene VALUES (9,  4,  5,  2,  'Bazaar chase scene through Peshawar Old City.',                11.0, 'Shooting')
  INTO scene VALUES (10, 5,  7,  1,  'Zara and Ahad meet in Karimabad — romantic encounter.',         7.5, 'Planned')
  INTO scene VALUES (11, 6,  8,  1,  'Jinnah addresses the nation at Lahore Fort.',                  20.0, 'Completed')
  INTO scene VALUES (12, 6, 13,  2,  'Jinnah walks through Faisal Mosque grounds — reflective.',     9.0, 'Completed')
  INTO scene VALUES (13, 7,  6,  1,  'Sameer performs on stage at grand concert.',                   13.0, 'Shooting')
  INTO scene VALUES (14, 7, 10,  2,  'Noor and Sameer argue in Shalimar Garden.',                     7.0, 'Planned')
  INTO scene VALUES (15, 8,  4,  1,  'Bilal faces his past in Islamabad apartment.',                  6.0, 'Planned')
  INTO scene VALUES (16, 9,  2,  1,  'DIG Asad chases suspect through Karachi Beach.',               15.0, 'Planned')
  INTO scene VALUES (17, 10, 9,  1,  'Meera and Imran reunite at Mohatta Palace.',                    8.5, 'Completed')
  INTO scene VALUES (18, 11,15,  1,  'Haunted forest sequence in Nathia Gali at night.',             17.0, 'Shooting')
  INTO scene VALUES (19, 12, 5,  1,  'Badshah Khan training montage in Peshawar Bazaar.',            12.0, 'Planned')
  INTO scene VALUES (20, 13,10,  1,  'Shalimar Garden romantic walk — final reconciliation.',         6.0, 'Planned')
SELECT * FROM dual;

-- ============================================================
-- CREW ASSIGNMENTS (15 rows)
-- ============================================================
INSERT ALL
  INTO crew_assignment VALUES (1,  1,  1,  'Director of Photography', TO_DATE('01-01-2025','DD-MM-YYYY'), TO_DATE('31-12-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (2,  1,  2,  'Production Manager',      TO_DATE('01-01-2025','DD-MM-YYYY'), TO_DATE('30-06-2026','DD-MM-YYYY'))
  INTO crew_assignment VALUES (3,  1,  4,  'VFX Supervisor',          TO_DATE('01-03-2025','DD-MM-YYYY'), TO_DATE('30-06-2026','DD-MM-YYYY'))
  INTO crew_assignment VALUES (4,  1,  7,  'Costume Designer',        TO_DATE('01-01-2025','DD-MM-YYYY'), TO_DATE('31-12-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (5,  2,  1,  'Cinematographer',         TO_DATE('01-02-2025','DD-MM-YYYY'), TO_DATE('30-09-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (6,  2,  5,  'Production Designer',     TO_DATE('01-02-2025','DD-MM-YYYY'), TO_DATE('30-09-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (7,  3,  6,  'Film Editor',             TO_DATE('01-03-2024','DD-MM-YYYY'), TO_DATE('31-03-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (8,  3,  3,  'Sound Engineer',          TO_DATE('01-03-2024','DD-MM-YYYY'), TO_DATE('31-12-2024','DD-MM-YYYY'))
  INTO crew_assignment VALUES (9,  4,  12, 'Camera Operator',         TO_DATE('01-03-2025','DD-MM-YYYY'), TO_DATE('31-08-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (10, 5,  15, 'Music Composer',          TO_DATE('01-06-2025','DD-MM-YYYY'), TO_DATE('28-02-2026','DD-MM-YYYY'))
  INTO crew_assignment VALUES (11, 6,  1,  'Cinematographer',         TO_DATE('01-01-2024','DD-MM-YYYY'), TO_DATE('31-12-2024','DD-MM-YYYY'))
  INTO crew_assignment VALUES (12, 6,  9,  'Head Makeup Artist',      TO_DATE('01-01-2024','DD-MM-YYYY'), TO_DATE('31-12-2024','DD-MM-YYYY'))
  INTO crew_assignment VALUES (13, 7,  15, 'Music Composer',          TO_DATE('01-04-2025','DD-MM-YYYY'), TO_DATE('31-12-2025','DD-MM-YYYY'))
  INTO crew_assignment VALUES (14, 9,  8,  'Stunt Coordinator',       TO_DATE('01-07-2025','DD-MM-YYYY'), TO_DATE('31-03-2026','DD-MM-YYYY'))
  INTO crew_assignment VALUES (15, 11, 10, 'Gaffer',                  TO_DATE('01-04-2025','DD-MM-YYYY'), TO_DATE('31-10-2025','DD-MM-YYYY'))
SELECT * FROM dual;

-- ============================================================
-- SCHEDULE (15 rows)
-- ============================================================
INSERT ALL
  INTO schedule VALUES (1,  1,  1,  3,  TO_DATE('10-02-2025','DD-MM-YYYY'), TO_DATE('10-02-2025 06:00','DD-MM-YYYY HH24:MI'), TO_DATE('10-02-2025 20:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (2,  1,  2,  8,  TO_DATE('18-02-2025','DD-MM-YYYY'), TO_DATE('18-02-2025 05:00','DD-MM-YYYY HH24:MI'), TO_DATE('18-02-2025 12:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (3,  1,  3,  5,  TO_DATE('25-02-2025','DD-MM-YYYY'), TO_DATE('25-02-2025 07:00','DD-MM-YYYY HH24:MI'), TO_DATE('25-02-2025 18:00','DD-MM-YYYY HH24:MI'), 'Scheduled')
  INTO schedule VALUES (4,  2,  4,  7,  TO_DATE('15-04-2025','DD-MM-YYYY'), TO_DATE('15-04-2025 16:00','DD-MM-YYYY HH24:MI'), TO_DATE('15-04-2025 22:00','DD-MM-YYYY HH24:MI'), 'Scheduled')
  INTO schedule VALUES (5,  2,  5,  4,  TO_DATE('20-04-2025','DD-MM-YYYY'), TO_DATE('20-04-2025 09:00','DD-MM-YYYY HH24:MI'), TO_DATE('20-04-2025 17:00','DD-MM-YYYY HH24:MI'), 'Scheduled')
  INTO schedule VALUES (6,  3,  6,  9,  TO_DATE('05-05-2024','DD-MM-YYYY'), TO_DATE('05-05-2024 20:00','DD-MM-YYYY HH24:MI'), TO_DATE('06-05-2024 03:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (7,  3,  7,  2,  TO_DATE('12-05-2024','DD-MM-YYYY'), TO_DATE('12-05-2024 22:00','DD-MM-YYYY HH24:MI'), TO_DATE('13-05-2024 05:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (8,  4,  8,  6,  TO_DATE('10-03-2025','DD-MM-YYYY'), TO_DATE('10-03-2025 10:00','DD-MM-YYYY HH24:MI'), TO_DATE('10-03-2025 16:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (9,  4,  9,  5,  TO_DATE('20-03-2025','DD-MM-YYYY'), TO_DATE('20-03-2025 08:00','DD-MM-YYYY HH24:MI'), TO_DATE('20-03-2025 19:00','DD-MM-YYYY HH24:MI'), 'In Progress')
  INTO schedule VALUES (10, 5,  10, 7,  TO_DATE('01-08-2025','DD-MM-YYYY'), TO_DATE('01-08-2025 17:00','DD-MM-YYYY HH24:MI'), TO_DATE('01-08-2025 21:00','DD-MM-YYYY HH24:MI'), 'Scheduled')
  INTO schedule VALUES (11, 6,  11, 8,  TO_DATE('15-03-2024','DD-MM-YYYY'), TO_DATE('15-03-2024 06:00','DD-MM-YYYY HH24:MI'), TO_DATE('15-03-2024 18:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (12, 6,  12, 13, TO_DATE('22-03-2024','DD-MM-YYYY'), TO_DATE('22-03-2024 07:00','DD-MM-YYYY HH24:MI'), TO_DATE('22-03-2024 14:00','DD-MM-YYYY HH24:MI'), 'Completed')
  INTO schedule VALUES (13, 7,  13, 6,  TO_DATE('10-05-2025','DD-MM-YYYY'), TO_DATE('10-05-2025 18:00','DD-MM-YYYY HH24:MI'), TO_DATE('10-05-2025 23:00','DD-MM-YYYY HH24:MI'), 'In Progress')
  INTO schedule VALUES (14, 11, 18, 15, TO_DATE('15-06-2025','DD-MM-YYYY'), TO_DATE('15-06-2025 21:00','DD-MM-YYYY HH24:MI'), TO_DATE('16-06-2025 04:00','DD-MM-YYYY HH24:MI'), 'Scheduled')
  INTO schedule VALUES (15, 12, 19, 5,  TO_DATE('01-02-2026','DD-MM-YYYY'), TO_DATE('01-02-2026 07:00','DD-MM-YYYY HH24:MI'), TO_DATE('01-02-2026 19:00','DD-MM-YYYY HH24:MI'), 'Scheduled')
SELECT * FROM dual;

COMMIT;
-- ============================================================
-- PHASE 4: VIEWS (Dashboards & Reporting)
-- ============================================================

-- View 1: Movie Overview with Director Name
CREATE OR REPLACE VIEW vw_movie_overview AS
SELECT m.movie_id, m.title, m.release_year, m.genre, m.rating, m.budget, m.status, 
       d.full_name AS director_name, d.nationality AS director_nationality
FROM movie m
JOIN director d ON m.director_id = d.director_id;

-- View 2: Full Cast Details and Billing
CREATE OR REPLACE VIEW vw_full_cast AS
SELECT m.title AS movie_title, a.full_name AS actor_name, a.nationality,
       cr.character_name, cr.role_type, cr.billing_order
FROM cast_role cr
JOIN movie m ON cr.movie_id = m.movie_id
JOIN actor a ON cr.actor_id = a.actor_id
ORDER BY m.title, cr.billing_order;

-- View 3: Active Shooting Schedule
CREATE OR REPLACE VIEW vw_active_schedule AS
SELECT s.schedule_id, m.title AS movie_title, sc.scene_no, sc.description AS scene_description,
       sl.name AS location_name, sl.country, s.shoot_date, s.status AS schedule_status
FROM schedule s
JOIN movie m ON s.movie_id = m.movie_id
JOIN scene sc ON s.scene_id = sc.scene_id
JOIN shooting_location sl ON s.location_id = sl.location_id
WHERE s.status IN ('Scheduled','In Progress');

-- View 4: Financial Health & Budget Tracking
CREATE OR REPLACE VIEW vw_movie_financials AS
SELECT m.movie_id, m.title, m.budget, NVL(SUM(c.total_value), 0) AS total_contract_expenses,
       (m.budget - NVL(SUM(c.total_value), 0)) AS remaining_budget
FROM movie m
LEFT JOIN contract c ON m.movie_id = c.movie_id
GROUP BY m.movie_id, m.title, m.budget;

-- View 5: Location Utilization and Screen Time
CREATE OR REPLACE VIEW vw_location_utilization AS
SELECT sl.location_id, sl.name AS location_name, sl.country, COUNT(s.scene_id) AS scenes_shot,
       SUM(s.duration) AS total_screen_time_minutes, (COUNT(s.scene_id) * sl.daily_rate) AS est_cost
FROM shooting_location sl
LEFT JOIN scene s ON sl.location_id = s.location_id
GROUP BY sl.location_id, sl.name, sl.country, sl.daily_rate;

-- View 6: Director Portfolio & Studio Investment
CREATE OR REPLACE VIEW vw_director_portfolio AS
SELECT d.full_name AS director_name, COUNT(m.movie_id) AS movies_directed, SUM(m.budget) AS managed_budget
FROM director d
LEFT JOIN movie m ON d.director_id = m.director_id
GROUP BY d.full_name;

-- View 7: Daily Call Sheet
CREATE OR REPLACE VIEW vw_daily_call_sheet AS
SELECT sch.shoot_date, m.title AS movie_title, sc.scene_no, sl.name AS location,
       TO_CHAR(sch.start_time, 'HH24:MI') AS call_time, TO_CHAR(sch.end_time, 'HH24:MI') AS wrap_time
FROM schedule sch
JOIN movie m ON sch.movie_id = m.movie_id
JOIN scene sc ON sch.scene_id = sc.scene_id
JOIN shooting_location sl ON sch.location_id = sl.location_id
WHERE sch.status != 'Postponed';

-- ============================================================
-- PHASE 5: JOINS (Multi-Table Data Extraction)
-- ============================================================

-- Join 1: Inner Join - Movies over $50M budget with director info
SELECT m.title, d.full_name AS director, m.budget, m.status
FROM movie m
JOIN director d ON m.director_id = d.director_id
WHERE m.budget > 50000000 ORDER BY m.budget DESC;

-- Join 2: Inner Join - Comprehensive Cast and Contract Details
SELECT m.title AS movie_name, a.full_name AS actor_name, cr.role_type, c.total_value
FROM movie m
JOIN cast_role cr ON m.movie_id = cr.movie_id
JOIN actor a ON cr.actor_id = a.actor_id
JOIN contract c ON m.movie_id = c.movie_id AND a.actor_id = c.party_id
WHERE c.party_type = 'Actor' ORDER BY m.title, c.total_value DESC;

-- Join 3: Left Join - Actors Not Yet Cast in Any Movie
SELECT a.actor_id, a.full_name, a.contact
FROM actor a
LEFT JOIN cast_role cr ON a.actor_id = cr.actor_id
WHERE cr.role_id IS NULL;

-- Join 4: Left Join - Locations and their Usage counts
SELECT sl.name, sl.country, COUNT(sc.scene_id) as scenes_planned
FROM shooting_location sl
LEFT JOIN scene sc ON sl.location_id = sc.location_id
GROUP BY sl.name, sl.country ORDER BY scenes_planned DESC;

-- Join 5: Multiple Joins - Crew Roster with active assignment durations
SELECT cm.full_name, m.title, ca.job_title, (ca.end_date - ca.start_date) AS assigned_days
FROM crew_member cm
JOIN crew_assignment ca ON cm.crew_id = ca.crew_id
JOIN movie m ON ca.movie_id = m.movie_id
ORDER BY assigned_days DESC;

-- Join 6: Full Outer Join - Check discrepancies between scenes and schedules
SELECT sc.scene_no, m.title, sch.schedule_id, sch.shoot_date
FROM scene sc
JOIN movie m ON sc.movie_id = m.movie_id
FULL OUTER JOIN schedule sch ON sc.scene_id = sch.scene_id;

-- ============================================================
-- PHASE 6: INDEPENDENT SUBQUERIES
-- ============================================================

-- Subquery 1: Movies with more than 1 lead actor (HAVING Subquery)
SELECT title FROM movie WHERE movie_id IN (
  SELECT movie_id FROM cast_role WHERE role_type = 'Lead' GROUP BY movie_id HAVING COUNT(*) > 1
);

-- Subquery 2: Actors who have not signed any contract (NOT IN)
SELECT full_name FROM actor WHERE actor_id NOT IN (
  SELECT party_id FROM contract WHERE party_type = 'Actor'
);

-- Subquery 3: Find movies with a budget higher than the studio average
SELECT title, budget, status FROM movie 
WHERE budget > (SELECT AVG(budget) FROM movie);

-- Subquery 4: Find all Actors acting in 'Sci-Fi' genre movies
SELECT full_name, nationality FROM actor WHERE actor_id IN (
  SELECT actor_id FROM cast_role WHERE movie_id IN (SELECT movie_id FROM movie WHERE genre = 'Sci-Fi')
);

-- Subquery 5: Find the shooting location with the highest daily rate
SELECT name, country, daily_rate FROM shooting_location
WHERE daily_rate = (SELECT MAX(daily_rate) FROM shooting_location);

-- Subquery 6: Find crew members assigned to the highest budget movie
SELECT cm.full_name, ca.job_title FROM crew_member cm
JOIN crew_assignment ca ON cm.crew_id = ca.crew_id
WHERE ca.movie_id = (SELECT movie_id FROM movie WHERE budget = (SELECT MAX(budget) FROM movie));

-- ============================================================
-- PHASE 7: CORRELATED SUBQUERIES
-- ============================================================

-- Correlated 1: Find the most expensive contract PER movie
SELECT m.title, c1.party_type, c1.total_value
FROM contract c1 JOIN movie m ON c1.movie_id = m.movie_id
WHERE c1.total_value = (SELECT MAX(c2.total_value) FROM contract c2 WHERE c2.movie_id = c1.movie_id);

-- Correlated 2: Find movies where total actor contracts exceed 20% of the total budget
SELECT m.title, m.budget
FROM movie m
WHERE (SELECT SUM(c.total_value) FROM contract c 
       WHERE c.movie_id = m.movie_id AND c.party_type = 'Actor') > (m.budget * 0.20);

-- Correlated 3: Find scenes that are longer than the average scene duration for their specific movie
SELECT m.title, sc.scene_no, sc.duration
FROM scene sc JOIN movie m ON sc.movie_id = m.movie_id
WHERE sc.duration > (SELECT AVG(sc2.duration) FROM scene sc2 WHERE sc2.movie_id = sc.movie_id);

-- Correlated 4: Find actors who are getting paid MORE than the average actor contract value
SELECT p.full_name, c.total_value, m.title
FROM contract c JOIN actor p ON c.party_id = p.actor_id JOIN movie m ON c.movie_id = m.movie_id
WHERE c.party_type = 'Actor' 
AND c.total_value > (SELECT AVG(total_value) FROM contract WHERE party_type = 'Actor');

-- Correlated 5: Identify locations that have more scheduled days than the average location
SELECT sl.name, (SELECT COUNT(*) FROM schedule s WHERE s.location_id = sl.location_id) as schedule_count
FROM shooting_location sl
WHERE (SELECT COUNT(*) FROM schedule s WHERE s.location_id = sl.location_id) > 
      (SELECT AVG(COUNT(*)) FROM schedule GROUP BY location_id);

-- ============================================================
-- PHASE 8: TRIGGERS (Automated Logic & Integrity)
-- ============================================================

-- ============================================================
-- PHASE 8: TRIGGERS (Automated Logic & Integrity)
-- ============================================================

-- 1. Create the base tables without IDENTITY
CREATE TABLE contract_log (
  log_id       NUMBER PRIMARY KEY,
  contract_id  NUMBER, 
  movie_id     NUMBER, 
  total_value  NUMBER(15,2), 
  logged_at    DATE DEFAULT SYSDATE
);

CREATE TABLE movie_audit_log (
  audit_id     NUMBER PRIMARY KEY,
  movie_id     NUMBER, 
  old_status   VARCHAR2(30), 
  new_status   VARCHAR2(30), 
  changed_by   VARCHAR2(50), 
  changed_at   DATE DEFAULT SYSDATE
);

-- 2. Create Sequences to generate the IDs
CREATE SEQUENCE seq_contract_log START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_movie_audit_log START WITH 1 INCREMENT BY 1;

-- 3. Create Triggers to auto-insert the generated IDs
CREATE OR REPLACE TRIGGER trg_auto_contract_log_id
BEFORE INSERT ON contract_log
FOR EACH ROW
BEGIN
  :NEW.log_id := seq_contract_log.NEXTVAL;
END;
/

CREATE OR REPLACE TRIGGER trg_auto_movie_audit_id
BEFORE INSERT ON movie_audit_log
FOR EACH ROW
BEGIN
  :NEW.audit_id := seq_movie_audit_log.NEXTVAL;
END;
/

-- Trigger 1: Auto-set release_year when movie status changes to 'Released'
CREATE OR REPLACE TRIGGER trg_set_release_year
BEFORE UPDATE ON movie FOR EACH ROW
BEGIN
  IF :NEW.status = 'Released' AND :OLD.status != 'Released' THEN
    :NEW.release_year := EXTRACT(YEAR FROM SYSDATE);
  END IF;
END;
/

-- Trigger 2: Prevent deleting a director who has active movies
CREATE OR REPLACE TRIGGER trg_protect_director
BEFORE DELETE ON director FOR EACH ROW
DECLARE v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM movie WHERE director_id = :OLD.director_id AND status NOT IN ('Released','Cancelled');
  IF v_count > 0 THEN RAISE_APPLICATION_ERROR(-20001, 'Cannot delete director with active movies.'); END IF;
END;
/

-- Trigger 3: Log when a contract value exceeds 10 million
CREATE OR REPLACE TRIGGER trg_high_value_contract
AFTER INSERT ON contract FOR EACH ROW
BEGIN
  IF :NEW.total_value > 10000000 THEN
    INSERT INTO contract_log (contract_id, movie_id, total_value) VALUES (:NEW.contract_id, :NEW.movie_id, :NEW.total_value);
  END IF;
END;
/

-- Trigger 4: Track history whenever a movie moves to a new production phase
CREATE OR REPLACE TRIGGER trg_movie_status_audit
AFTER UPDATE OF status ON movie FOR EACH ROW
BEGIN
  IF :OLD.status != :NEW.status THEN
    INSERT INTO movie_audit_log (movie_id, old_status, new_status, changed_by)
    VALUES (:NEW.movie_id, :OLD.status, :NEW.status, USER);
  END IF;
END;
/

-- Trigger 5: Prevent overlapping schedules at the same shooting location
CREATE OR REPLACE TRIGGER trg_prevent_location_overlap
BEFORE INSERT OR UPDATE ON schedule FOR EACH ROW
DECLARE v_overlap_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_overlap_count FROM schedule
  WHERE location_id = :NEW.location_id AND schedule_id != NVL(:NEW.schedule_id, 0)
    AND ((:NEW.start_time BETWEEN start_time AND end_time) OR (:NEW.end_time BETWEEN start_time AND end_time));
  IF v_overlap_count > 0 THEN RAISE_APPLICATION_ERROR(-20002, 'Location already booked for this time frame.'); END IF;
END;
/

-- Trigger 6: Enforce chronological dates on Crew Assignments
CREATE OR REPLACE TRIGGER trg_check_crew_dates
BEFORE INSERT OR UPDATE ON crew_assignment FOR EACH ROW
BEGIN
  IF :NEW.start_date > :NEW.end_date THEN RAISE_APPLICATION_ERROR(-20005, 'Start date cannot be after end date.'); END IF;
END;
/

-- Trigger 7: Prevent negative budgets
CREATE OR REPLACE TRIGGER trg_check_positive_budget
BEFORE INSERT OR UPDATE ON movie FOR EACH ROW
BEGIN
  IF :NEW.budget <= 0 THEN RAISE_APPLICATION_ERROR(-20004, 'Movie budget must be greater than zero.'); END IF;
END;
/

-- ============================================================
-- PHASE 9: PL/SQL STORED PROCEDURES & FUNCTIONS
-- ============================================================

-- Function 1: Calculate remaining budget
CREATE OR REPLACE FUNCTION func_get_remaining_budget (p_movie_id IN NUMBER) RETURN NUMBER IS
  v_budget NUMBER; v_spent NUMBER;
BEGIN
  SELECT budget INTO v_budget FROM movie WHERE movie_id = p_movie_id;
  SELECT NVL(SUM(total_value), 0) INTO v_spent FROM contract WHERE movie_id = p_movie_id;
  RETURN (v_budget - v_spent);
EXCEPTION WHEN NO_DATA_FOUND THEN RETURN 0;
END;
/

-- Function 2: Calculate Total Days a Location is Booked
CREATE OR REPLACE FUNCTION func_location_booked_days(p_loc_id IN NUMBER) RETURN NUMBER IS
  v_total_days NUMBER;
BEGIN
  SELECT COUNT(DISTINCT TRUNC(shoot_date)) INTO v_total_days FROM schedule WHERE location_id = p_loc_id AND status != 'Cancelled';
  RETURN v_total_days;
EXCEPTION WHEN NO_DATA_FOUND THEN RETURN 0;
END;
/

-- Procedure 1: Get all cast for a given movie
CREATE OR REPLACE PROCEDURE proc_get_cast (p_movie_id IN NUMBER) IS
  CURSOR c_cast IS SELECT a.full_name, cr.character_name, cr.role_type FROM cast_role cr JOIN actor a ON cr.actor_id = a.actor_id WHERE cr.movie_id = p_movie_id ORDER BY cr.billing_order;
BEGIN
  FOR rec IN c_cast LOOP DBMS_OUTPUT.PUT_LINE('Actor: ' || rec.full_name || ' | Char: ' || rec.character_name || ' | Role: ' || rec.role_type); END LOOP;
END;
/

-- Procedure 2: Assign crew member to a movie dynamically
CREATE OR REPLACE PROCEDURE proc_assign_crew (p_movie_id IN NUMBER, p_crew_id IN NUMBER, p_job_title IN VARCHAR2, p_start IN DATE, p_end IN DATE) IS
  v_new_id NUMBER;
BEGIN
  SELECT NVL(MAX(assignment_id), 0) + 1 INTO v_new_id FROM crew_assignment;
  INSERT INTO crew_assignment VALUES (v_new_id, p_movie_id, p_crew_id, p_job_title, p_start, p_end);
  COMMIT; DBMS_OUTPUT.PUT_LINE('Crew assigned successfully. ID: ' || v_new_id);
END;
/

-- Procedure 3: Print full movie summary dashboard
CREATE OR REPLACE PROCEDURE proc_movie_summary (p_movie_id IN NUMBER) IS
  v_title movie.title%TYPE; v_budget movie.budget%TYPE; v_dir director.full_name%TYPE; v_cast NUMBER; v_scene NUMBER;
BEGIN
  SELECT m.title, m.budget, d.full_name INTO v_title, v_budget, v_dir FROM movie m JOIN director d ON m.director_id = d.director_id WHERE m.movie_id = p_movie_id;
  SELECT COUNT(*) INTO v_cast FROM cast_role WHERE movie_id = p_movie_id;
  SELECT COUNT(*) INTO v_scene FROM scene WHERE movie_id = p_movie_id;
  DBMS_OUTPUT.PUT_LINE('Title: ' || v_title || ' | Dir: ' || v_dir || ' | Budget: $' || v_budget || ' | Cast: ' || v_cast || ' | Scenes: ' || v_scene);
END;
/

-- Procedure 4: Delay production by shifting schedule dates
CREATE OR REPLACE PROCEDURE proc_delay_production (p_movie_id IN NUMBER, p_delay_days IN NUMBER) IS
  v_count NUMBER := 0;
BEGIN
  UPDATE schedule SET shoot_date = shoot_date + p_delay_days, start_time = start_time + p_delay_days, end_time = end_time + p_delay_days, status = 'Postponed'
  WHERE movie_id = p_movie_id AND status IN ('Scheduled', 'In Progress');
  v_count := SQL%ROWCOUNT; COMMIT;
  DBMS_OUTPUT.PUT_LINE('Delayed ' || v_count || ' scheduled scenes by ' || p_delay_days || ' days.');
END;
/

-- Procedure 5: Fire/Terminate crew member early
CREATE OR REPLACE PROCEDURE proc_terminate_crew (p_crew_id IN NUMBER, p_movie_id IN NUMBER) IS
BEGIN
  UPDATE crew_assignment SET end_date = SYSDATE WHERE crew_id = p_crew_id AND movie_id = p_movie_id AND end_date > SYSDATE;
  IF SQL%ROWCOUNT > 0 THEN DBMS_OUTPUT.PUT_LINE('Crew member terminated from Movie ID ' || p_movie_id); COMMIT; END IF;
END;
/

-- Procedure 6: Estimate Location Costs for a Movie
CREATE OR REPLACE PROCEDURE proc_estimate_location_costs(p_movie_id IN NUMBER) IS
  CURSOR c_locs IS SELECT sl.name, sl.daily_rate, COUNT(sch.schedule_id) AS days FROM schedule sch JOIN shooting_location sl ON sch.location_id = sl.location_id WHERE sch.movie_id = p_movie_id GROUP BY sl.name, sl.daily_rate;
  v_total NUMBER := 0; v_sub NUMBER;
BEGIN
  FOR loc IN c_locs LOOP
    v_sub := loc.daily_rate * loc.days; v_total := v_total + v_sub;
    DBMS_OUTPUT.PUT_LINE('Loc: ' || loc.name || ' | Cost: $' || v_sub);
  END LOOP;
  DBMS_OUTPUT.PUT_LINE('TOTAL COST: $' || v_total);
END;
/

-- ============================================================
-- PHASE 10: TEST EXECUTIONS
-- ============================================================
-- Enable output printing (if your interface supports the command line)
SET SERVEROUTPUT ON;

-- Test 1: proc_get_cast (Prints the cast list for Movie 1)
BEGIN
  proc_get_cast(1);
END;
/

-- Test 2: proc_movie_summary (Prints a dashboard for Movie 3)
BEGIN
  proc_movie_summary(3);
END;
/

-- Test 3: proc_assign_crew (Assigns crew member 3 to movie 2)
BEGIN
  proc_assign_crew(2, 3, 'Sound Engineer', TO_DATE('01-07-2025','DD-MM-YYYY'), TO_DATE('30-11-2025','DD-MM-YYYY'));
END;
/

-- Test 4: proc_delay_production (Delays all scheduled scenes for Movie 3 by 14 days)
BEGIN
  proc_delay_production(3, 14);
END;
/

-- Test 5: proc_terminate_crew (Fires crew member 1 from movie 1 by setting their end_date to today)
BEGIN
  proc_terminate_crew(1, 1);
END;
/

-- Test 6: proc_estimate_location_costs (Calculates total location costs based on schedule for Movie 1)
BEGIN
  proc_estimate_location_costs(1);
END;
/


-- Test 1: func_get_remaining_budget (Calculates budget minus total contracts for Movie 1)
SELECT func_get_remaining_budget(1) AS remaining_budget FROM DUAL;

-- Test 2: func_location_booked_days (Counts how many distinct days Location 1 is used)
SELECT func_location_booked_days(1) AS total_booked_days FROM DUAL;


-- Test 1: trg_set_release_year AND trg_movie_status_audit
-- Action: Change movie 4 status to 'Released'
UPDATE movie SET status = 'Released' WHERE movie_id = 4;
COMMIT;

-- Verify Trigger 1 & 4 worked:
-- The release year should now be updated to the current year (2025/2026)
SELECT title, release_year, status FROM movie WHERE movie_id = 4;
-- The audit log should show the old status vs new status
SELECT * FROM movie_audit_log;


-- Test 2: trg_high_value_contract (and the 11g auto-ID trigger)
-- Action: Insert a contract worth 15 Million (Over the 10M threshold)
INSERT INTO contract (contract_id, movie_id, party_id, party_type, signed_date, expiry_date, total_value, terms_summary) 
VALUES (99, 2, 1, 'Actor', SYSDATE, SYSDATE+30, 15000000, 'High value test contract');
COMMIT;

-- Verify Trigger 2 worked: 
-- The contract_log table should have caught this insertion
SELECT * FROM contract_log;


-- Test 1: trg_set_release_year AND trg_movie_status_audit
-- Action: Change movie 4 status to 'Released'
UPDATE movie SET status = 'Released' WHERE movie_id = 4;
COMMIT;

-- Verify Trigger 1 & 4 worked:
-- The release year should now be updated to the current year (2025/2026)
SELECT title, release_year, status FROM movie WHERE movie_id = 4;
-- The audit log should show the old status vs new status
SELECT * FROM movie_audit_log;



-- ============================================================
-- END OF SCRIPT
-- ============================================================