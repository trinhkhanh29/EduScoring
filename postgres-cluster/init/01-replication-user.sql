-- Tạo replication user
CREATE USER replicator WITH REPLICATION ENCRYPTED PASSWORD 'replicator_pass';

-- Tạo replication slot cho từng follower (giữ WAL không bị xóa khi đang clone)
SELECT pg_create_physical_replication_slot('slot_follower_1');
SELECT pg_create_physical_replication_slot('slot_follower_2');

-- Thêm rule replication vào pg_hba.conf
DO $$
DECLARE
  hba_path text;
BEGIN
  SELECT setting INTO hba_path FROM pg_settings WHERE name = 'hba_file';
  EXECUTE format(
    'COPY (VALUES (''host replication replicator 0.0.0.0/0 trust'')) TO PROGRAM %L',
    'tee -a ' || hba_path
  );
  PERFORM pg_reload_conf();
  RAISE NOTICE '[LEADER] Setup xong.';
END;
$$;