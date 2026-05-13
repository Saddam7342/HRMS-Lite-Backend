# HRMS is split into three sibling folders under Documents (adjust paths if needed):
#   HRMS                      — .NET API (this repo)
#   HRMS-Admin-frontend-Web   — Vite + React admin UI
#   HRMS-Employee-Mobile      — Expo employee app
#
# Create empty GitHub repositories (via web UI or gh CLI), then push each folder:
#
#   cd "$env:USERPROFILE\Documents\HRMS-Admin-frontend-Web"
#   git remote add origin https://github.com/<org>/HRMS-Admin-frontend-Web.git
#   git branch -M main
#   git push -u origin main
#
#   cd "$env:USERPROFILE\Documents\HRMS-Employee-Mobile"
#   git remote add origin https://github.com/<org>/HRMS-Employee-Mobile.git
#   git branch -M main
#   git push -u origin main
#
# This backend repo: rename remote repo if you want (e.g. HRMS-API), then:
#   git add -A
#   git commit -m "Remove embedded admin frontend; lives in HRMS-Admin-frontend-Web"
#   git push
