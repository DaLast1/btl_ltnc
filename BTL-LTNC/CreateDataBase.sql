create database QLTX;
use QLTX;

create table Xe
(
	BienSo char(5) not null,
	LoaiXe nvarchar(50) not null,
	Hang nvarchar(50) not null,
	KieuXe nvarchar(50) not null,
	GiaThue int not null,
	TrangThai nvarchar(50),
	primary key (BienSo),
	check (GiaThue > 0)
);

create table Khach
(
	Mak char(5) not null,
	TenK nvarchar(50) not null,
	GioiTinh nvarchar(10) not null,
	SDT char(10) not null,
	DiaChi nvarchar(50) not null
	primary key (MaK),
);

create table HoaDon
(
	MaHD char(5) not null, 
	MaK char(5) not null,
	BienSo char(5) not null,
	LoaiHinhThue nvarchar(50) not null,
	TGThue decimal(2, 1) not null,
	TGBD datetime not null,
	TGKT datetime not null,
	ThanhTien int not null,
	TinhTrang nvarchar(50) not null,
	primary key (MaHD),
	foreign key (MaK) references Khach(MaK),
	foreign key (BienSo) references Xe(BienSo),
	check (ThanhTien >= 0)
);