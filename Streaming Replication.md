# PostgreSQL Streaming Replication — Local Cluster

> Cụm PostgreSQL 1 Leader + 2 Follower chạy bằng Docker Compose.  
> Mục đích: học tập, demo kiến trúc database replication, ghi vào CV/portfolio.

---

## Kiến trúc

```
                    ┌─────────────────┐
                    │   pg-leader     │
                    │   port 5432     │  ← Đọc + Ghi (Primary)
                    │   172.18.0.2    │
                    └────────┬────────┘
                             │  WAL Streaming (real-time)
              ┌──────────────┴──────────────┐
              ▼                             ▼
   ┌─────────────────┐           ┌─────────────────┐
   │  pg-follower-1  │           │  pg-follower-2  │
   │  port 5433      │           │  port 5434      │  ← Chỉ đọc (Standby)
   │  172.18.0.3     │           │  172.18.0.4     │
   └─────────────────┘           └─────────────────┘
```

**Leader** — nhận mọi write, stream WAL sang Follower liên tục.  
**Follower** — clone data từ Leader lúc khởi động, sau đó nhận WAL real-time. Từ chối mọi lệnh ghi (`ERROR: cannot execute INSERT in a read-only transaction`).

---

## Cấu trúc thư mục

```
postgres-cluster/
├── docker-compose.yml
└── init/
    └── 01-replication-user.sql   ← Tạo user replicator + replication slots
```

---

## Khái niệm cốt lõi

### WAL (Write-Ahead Log)
Mọi thay đổi dữ liệu trên Leader đều được ghi vào WAL trước khi apply vào data file. Follower nhận WAL này và replay lại — đây là cơ chế replication.

### Replication Slot
Giữ WAL trên Leader không bị xóa cho đến khi Follower tương ứng đã nhận xong. Không có slot → WAL bị cleanup → Follower clone thất bại với lỗi `WAL segment has already been removed`.

### pg_basebackup
Tool clone toàn bộ data từ Leader sang Follower lần đầu. Flag `-R` tự tạo `standby.signal` + `primary_conninfo` để Follower biết phải kết nối lại Leader sau khi clone xong.

### hot_standby
Cho phép Follower nhận query đọc trong khi đang replay WAL. Không có flag này Follower sẽ từ chối mọi connection.

---

## Cài đặt & Chạy

### Yêu cầu
- Docker Desktop đang chạy
- PowerShell (Windows) hoặc terminal (Mac/Linux)

### Bước 1 — Tạo thư mục

```powershell
mkdir postgres-cluster
cd postgres-cluster
mkdir init
```

### Bước 2 — Tạo 2 file config

Tạo `init/01-replication-user.sql`:
```sql
CREATE USER replicator WITH REPLICATION ENCRYPTED PASSWORD 'replicator_pass';
SELECT pg_create_physical_replication_slot('slot_follower_1');
SELECT pg_create_physical_replication_slot('slot_follower_2');

DO $$
DECLARE hba_path text;
BEGIN
  SELECT setting INTO hba_path FROM pg_settings WHERE name = 'hba_file';
  EXECUTE format(
    'COPY (VALUES (''host replication replicator 0.0.0.0/0 trust'')) TO PROGRAM %L',
    'tee -a ' || hba_path
  );
  PERFORM pg_reload_conf();
END;
$$;
```

Tạo `docker-compose.yml` — xem file đính kèm trong repo.

### Bước 3 — Khởi động

```powershell
docker compose up -d
```

### Bước 4 — Theo dõi Follower clone

```powershell
docker logs -f pg-follower-1
```

Thành công khi thấy:
```
[FOLLOWER-1] Clone xong.
LOG:  entering standby mode
LOG:  database system is ready to accept read-only connections
LOG:  started streaming WAL from primary at 0/5000000 on timeline 1
```

### Bước 5 — Verify replication

```powershell
docker exec pg-leader psql -U admin -d eduscoring -c "SELECT client_addr, state, sent_lsn, replay_lsn FROM pg_stat_replication;"
```

Kết quả mong đợi — 2 dòng `streaming`, `sent_lsn = replay_lsn` (không có độ trễ):
```
 client_addr |   state   | sent_lsn  | replay_lsn
-------------+-----------+-----------+------------
 172.18.0.3  | streaming | 0/5041360 | 0/5041360
 172.18.0.4  | streaming | 0/5041360 | 0/5041360
```

---

## Test replication

```powershell
# Ghi vào Leader
docker exec pg-leader psql -U admin -d eduscoring -c "CREATE TABLE test_repl (id serial, msg text, ts timestamptz default now());"
docker exec pg-leader psql -U admin -d eduscoring -c "INSERT INTO test_repl (msg) VALUES ('Hello from Leader');"

# Đọc từ Follower — thấy data ngay lập tức
docker exec pg-follower-1 psql -U admin -d eduscoring -c "SELECT * FROM test_repl;"

# Thử ghi vào Follower — bị từ chối
docker exec pg-follower-1 psql -U admin -d eduscoring -c "INSERT INTO test_repl (msg) VALUES ('Try write');"
# → ERROR: cannot execute INSERT in a read-only transaction
```

---

## Failover thủ công (khi Leader chết)

Streaming Replication thuần **không tự động failover**. Khi Leader mất, cần promote Follower lên bằng tay.

### Bước 1 — Giả lập Leader chết

```powershell
docker stop pg-leader
```

### Bước 2 — Quan sát Follower phản ứng

```powershell
docker logs pg-follower-1 --tail 10
```

Follower phát hiện mất kết nối và chờ:
```
FATAL: could not send end-of-streaming message to primary
FATAL: could not connect to the primary server: Connection refused
LOG:   waiting for WAL to become available at 0/...
```

### Bước 3 — Promote Follower-1 lên Leader mới

```powershell
docker exec pg-follower-1 su postgres -c "pg_ctl promote -D /var/lib/postgresql/data"
# → waiting for server to promote.... done
# → server promoted
```

### Bước 4 — Xác nhận promote thành công

```powershell
docker exec pg-follower-1 psql -U admin -d eduscoring -c "SELECT pg_is_in_recovery();"
```

```
 pg_is_in_recovery
-------------------
 f                   ← false = đây là Leader, không còn là standby
```

### Bước 5 — Ghi vào Leader mới

```powershell
docker exec pg-follower-1 psql -U admin -d eduscoring -c "INSERT INTO test_repl (msg) VALUES ('Written to NEW leader');"
docker exec pg-follower-1 psql -U admin -d eduscoring -c "SELECT * FROM test_repl;"
```

```
 id |          msg          |              ts
----+-----------------------+-------------------------------
  1 | Hello from Leader     | 2026-04-12 15:38:13.995395+00
 34 | Written to NEW leader | 2026-04-12 15:49:23.927409+00
```

Data cũ vẫn còn, ghi mới thành công.

### Trạng thái Follower-2 sau failover

Follower-2 vẫn đang cố kết nối Leader cũ đã chết:
```
FATAL: could not translate host name "pg-leader" to address: Name or service not known
LOG:   waiting for WAL to become available at 0/...
```

Để Follower-2 nhận Leader mới cần cập nhật `primary_conninfo` trong `postgresql.auto.conf` — hoặc dùng **Patroni** để tự động hóa toàn bộ quá trình này.

---

## Giới hạn của Streaming Replication thuần

| Tính năng | Streaming Replication | Patroni + etcd |
|---|---|---|
| Replication real-time | ✅ | ✅ |
| Failover tự động | ❌ Thủ công | ✅ Tự động |
| Bầu Leader mới | ❌ Tay chạy pg_promote | ✅ Tự động |
| Redirect traffic | ❌ Tay đổi connection string | ✅ Qua VIP/HAProxy |
| Độ phức tạp setup | Thấp | Cao hơn |

Streaming Replication thuần phù hợp để **hiểu cơ chế** và **demo portfolio**. Production thực tế dùng thêm Patroni hoặc pgBouncer để có auto-failover.

---

## Connection string cho ứng dụng

```json
"ConnectionStrings": {
  "WriteDb": "Host=localhost;Port=5432;Database=eduscoring;Username=admin;Password=secret123",
  "ReadDb":  "Host=localhost;Port=5433;Database=eduscoring;Username=admin;Password=secret123"
}
```

`WriteDb` → Leader (port 5432), dùng cho mọi INSERT/UPDATE/DELETE — Commands trong CQRS.  
`ReadDb` → Follower (port 5433), dùng cho SELECT — Queries trong CQRS.

---

## Dừng và xóa cluster

```powershell
# Dừng, giữ nguyên data
docker compose down

# Dừng và xóa toàn bộ data (reset hoàn toàn)
docker compose down -v
```

---

## Liên quan

- Dự án chính: [EduScoring](../README.md) — hệ thống chấm điểm tự luận với Vertical Slice Architecture
- [PostgreSQL Streaming Replication — Official Docs](https://www.postgresql.org/docs/current/warm-standby.html)
- [pg_basebackup](https://www.postgresql.org/docs/current/app-pgbasebackup.html)
