# Git Setup Guide - Fixing .vs/ Lock Issues

## ?? V?n ??

Visual Studio ?ang lock các file trong th? m?c `.vs/` (??c bi?t là `*.vsidx`), khi?n Git không th? ??c/index ???c các file này.

**L?i th??ng g?p:**
```
error: open(".vs/..."): Permission denied
fatal: Unable to process path .vs/...
```

## ? Gi?i Pháp

### B??c 1: ?óng Visual Studio
```powershell
# ??m b?o Visual Studio ?ã ?óng hoàn toàn
# Ki?m tra Task Manager không còn process devenv.exe ho?c ServiceHub.*.exe
```

### B??c 2: Xóa cache Git cho các file ?ã tracked

N?u `.vs/` ?ã ???c tracked tr??c ?ó:

```powershell
# Di chuy?n ??n th? m?c root c?a repository
cd C:\Users\Admin\source\repos\DemoGeoServer

# Remove t? Git index (nh?ng gi? file trên disk)
git rm -r --cached .vs/
git rm -r --cached */bin/
git rm -r --cached */obj/
git rm -r --cached */*.user

# Ho?c xóa toàn b? và add l?i
git rm -r --cached .
```

### B??c 3: Add l?i v?i .gitignore m?i

```powershell
# Add t?t c? files (gitignore s? t? ??ng lo?i tr? .vs/)
git add .

# Ki?m tra nh?ng file s? ???c commit
git status

# N?u v?n th?y .vs/ trong danh sách, ki?m tra l?i .gitignore
```

### B??c 4: Commit changes

```powershell
git commit -m "Add .gitignore and remove tracked VS cache files"
```

## ?? Verify .gitignore ho?t ??ng

```powershell
# Ki?m tra xem .vs/ có b? ignore không
git check-ignore -v .vs/

# Output mong ??i:
# .gitignore:36:.vs/    .vs/

# Ki?m tra status
git status

# Không nên th?y .vs/ trong untracked files
```

## ?? Git Commands H?u Ích

### Clean untracked files (c?n th?n!)
```powershell
# Xem nh?ng file s? b? xóa
git clean -n -d

# Xóa untracked files và directories
git clean -f -d

# Xóa k? c? ignored files (NGUY HI?M - s? xóa bin/obj/)
git clean -f -d -x
```

### Force add n?u c?n (không khuy?n khích)
```powershell
# N?u th?t s? c?n add file b? ignore
git add -f .vs/some-important-file
```

### Ki?m tra ignored files
```powershell
# List t?t c? ignored files
git status --ignored

# Ki?m tra specific file/folder có b? ignore không
git check-ignore .vs/
git check-ignore bin/Debug/
```

## ?? Checklist Sau Khi Setup

- [ ] File `.gitignore` ?ã ???c t?o trong root
- [ ] ?ã ?óng Visual Studio
- [ ] Ch?y `git rm -r --cached .vs/`
- [ ] Ch?y `git add .`
- [ ] Verify v?i `git status` - không th?y `.vs/`
- [ ] Commit changes
- [ ] M? l?i Visual Studio
- [ ] Th? `git status` l?n n?a - v?n không th?y `.vs/`

## ??? Troubleshooting

### V?n th?y .vs/ trong git status

**Nguyên nhân:**
- File ?ã ???c tracked tr??c khi có .gitignore
- .gitignore syntax sai

**Gi?i pháp:**
```powershell
# Xóa kh?i Git index
git rm -r --cached .vs/

# Ki?m tra .gitignore có dòng `.vs/` không
cat .gitignore | Select-String ".vs/"

# Re-add
git add .
```

### Permission denied khi git rm

**Nguyên nhân:**
- Visual Studio v?n ?ang ch?y
- File ?ang ???c process khác s? d?ng

**Gi?i pháp:**
```powershell
# 1. ?óng Visual Studio hoàn toàn
# 2. Check process
Get-Process | Where-Object {$_.ProcessName -like "*devenv*" -or $_.ProcessName -like "*ServiceHub*"}

# 3. Kill n?u c?n (c?n th?n!)
Stop-Process -Name "devenv" -Force

# 4. Th? l?i
git rm -r --cached .vs/
```

### File v?n b? track sau khi add .gitignore

**Nguyên nhân:**
- .gitignore ch? ignore untracked files
- Files ?ã tracked tr??c ?ó v?n ???c Git theo dõi

**Gi?i pháp:**
```powershell
# Ph?i remove kh?i index tr??c
git rm --cached <file>

# Ho?c toàn b?
git rm -r --cached .
git add .
```

## ?? Git Best Practices

### 1. Luôn có .gitignore t? ??u
- T?o `.gitignore` tr??c khi `git init`
- Ho?c ngay sau `git init`

### 2. Không commit sensitive files
```gitignore
# ?ã có trong .gitignore
appsettings.Development.json
appsettings.Local.json
secrets.json
*.pfx
```

### 3. Review tr??c khi commit
```powershell
# Xem nh?ng file s? ???c commit
git status

# Xem diff
git diff --staged
```

### 4. Commit message rõ ràng
```powershell
# T?t
git commit -m "Add user authentication with JWT"
git commit -m "Fix: Remove .vs/ from tracking"

# Không t?t
git commit -m "update"
git commit -m "fix bug"
```

## ?? Quick Recovery Commands

```powershell
# N?u ?ã commit nh?m .vs/
git rm -r --cached .vs/
git commit --amend -m "Add .gitignore and clean up tracked files"

# N?u c?n revert commit
git reset --soft HEAD~1

# N?u c?n undo staged files
git restore --staged .

# N?u c?n discard local changes
git restore .
```

## ?? Additional Resources

- [GitHub .gitignore templates](https://github.com/github/gitignore)
- [Visual Studio .gitignore](https://github.com/github/gitignore/blob/main/VisualStudio.gitignore)
- [Git Documentation](https://git-scm.com/doc)

## ?? Important Notes

1. **Luôn backup** tr??c khi ch?y `git clean -f -d -x`
2. **?óng Visual Studio** tr??c khi xóa `.vs/` kh?i Git
3. **Review** `git status` tr??c khi commit
4. **Không commit** passwords, secrets, connection strings vào Git
5. **Use** environment variables ho?c User Secrets cho sensitive data

---

**Created for:** DemoGeoServer Project  
**Git Version:** Tested with Git 2.x  
**Visual Studio:** 2022
