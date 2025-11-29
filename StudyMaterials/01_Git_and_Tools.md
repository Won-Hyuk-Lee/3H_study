## 1. Git 기초
Git은 버전 관리 시스템으로 형상관리라고도 한다. 게임의 세이브 포인트와 같다

### 핵심 개념
- **Repository (저장소)**: 프로젝트 파일과 변경 이력이 저장되는 곳.
- **Commit (커밋)**: 현재 상태를 저장하는 행위 (세이브).
- **Push (푸시)**: 내 컴퓨터의 커밋들을 원격 저장소(GitHub)로 업로드.
- **Pull (풀)**: 원격 저장소의 변경 사항을 내 컴퓨터로 가져오기.

### 필수 워크플로우
1. **사전 작업(초기작업)**: `git`의 아이디 및 유저이름 등록하기.
git config user.name	# 이름 확인
git config user.email	# 이메일 확인

git config --global user.name	"홍길동"	# 이름 변경
git config --global user.email	"gildongHong@test.com"	# 이메일 변경

# 설정된 사용자를 지울 때,
git config --unset user.name	# 이름 삭제
git config --unset user.email	# 이메일 삭제

# gloabal로 설정된 config 사용자를 지울 경우,
git config --unset --global user.name
git config --unset --global user.email


2. **작업**:
git status -> 현재 상태 확인
git log -> 지금까지 쌓여있는 커밋들의 확인 및 현재 헤더 확인
git add -> 현재까지 작업한 것들을 저장하기 전에 스테이징(무대에 올림)
git commit -m "커밋제목" -m "커밋내용" -> 무대에 올린 작업내용들의 제목과 내용을 작성
git commit --amend -m "변경하고싶은 커밋제목" -m "변경하고싶은 커밋내용" -> 가장마지막 커밋의 커밋 제목 및 메시지 변경
git push -> 변경 내역을 원격 저장소에 전송

git branch "브랜치명" -> 브랜치 생성
git branch -d 브랜치명 -> 브랜치 삭제

git switch "브랜치명" -> 해당 브랜치로 이동
git pull -> 원격 저장소에서 변동사항 받아옴 (최신 수정코드가 있으면 코드내용도 받아옴)
git fetch -> 원격 저장소에서 변동사항을 받아옴 (내역만받고 코드내용 변동은 받지않음)

git merge "합치고 싶은 브랜치명" -> 현재 브랜치와 합치고 싶은 브랜치의 내용을 합친다.
git merge --abort -> merge 하다가 중간에 취소하고 나가고 싶을때

git stash -> 마지막 commit 상태에서 지금까지 진행된 작업을 임시공간으로 보내고 가장 마지막 commit 상태로 변경(주로 임시저장해두고 다른 브랜치 이동할때 씀)
git stash list -> 임시저장한 것들 보기
git stash apply -> 임시저장한거 가져오기

git checkout -t origin/브랜치명 -> 원격 저장소에 있는 브랜치를 로컬에 가져옴
git clone url -> 원격 저장소 복제

git remote add origin URL -> 원격 저장소 연결
git remote -v -> 원격 저장소 확인

git log --oneline -> 간결하게 로그보기
git log --graph -> 브랜치 그래프로 보기
git diff -> 변경사항 비교

실수복구관련
git restore 파일명 -> 해당 파일을 마지막 커밋 상태로 돌림
git restore --staged 파일명 -> git add로 스테이징 올린 것 취소
git reset 커밋해시 -> 해당 커밋상태로 되돌림
git reset --hard 커밋해시 -> 해당 커밋상태로 강제로 되돌리고 기존에 있는 것조차 날려버림

--협업관련--
pr(pull request 설명)
pull request는 내가 작업한 코드를 팀의 메인 코드에 합치기 전에 이거 ㄱㅊ? 하고 질문하는것

pr 과정으로는 내 feature 브랜치에서 작업을 완료 후 push하고
git hub에서 pull request를 생성한 뒤 리뷰어가 검토 및 피드백을 줌
수정이 필요하면 추가 커밋(작업)을 한 뒤 push하고 승인이 나면 develop 브랜치에 auto merge
작업이 완료된 브랜치는 삭제.

Git Commit 컨벤션
커밋 메시지를 일관되게 작성하면 나중에 히스토리 보기가 편함
타입: 제목 (50자 이내)
본문 (선택사항, 무엇을 왜 변경했는지)

주요 타입:
feat: 새로운 기능 추가
fix: 버그 수정
refactor: 코드 리팩토링 (기능 변경 없이 코드 개선)
style: 코드 포맷팅, 세미콜론 누락 등 (기능 변경 없음)
docs: 문서 수정
test: 테스트 코드 추가/수정
chore: 빌드 설정, 패키지 매니저 설정 등

좋은 예시:
git commit -m "feat: 사용자 로그인 기능 추가"
git commit -m "fix: 로그인 시 비밀번호 검증 오류 수정"
git commit -m "refactor: 사용자 인증 로직 함수로 분리"
git commit -m "style: 코드 들여쓰기 정리"
git commit -m "docs: README에 설치 방법 추가"
나쁜 예시:
git commit -m "수정"  # 무엇을 수정했는지 알 수 없음
git commit -m "ㅁㄴㅇㄹ"  # 의미 없는 메시지
git commit -m "버그고침"  # 어떤 버그인지 불명확
팁:
제목은 명령문으로 작성 ("추가함" ❌ → "추가" ⭕)
한 커밋에는 하나의 의미있는 변경사항만
커밋은 자주, 작게 쪼개서 하는 것이 좋음
예를들면 1층에서 100층까지 건물을 올린다고 생각해보자. 만약 1층마다 커밋을 하면 귀찮긴 하겠지만 추후 작업을 진행하는데 56층에 문제가 생겼으면 해당 56층 커밋을 확인해서 그걸 날리면 그만인데 
5층이나 10층씩 작업을 했다면 날릴때 1층만 날리지 못하고 5층 혹은 10층을 날려야 하는 문제가 발생할 수 있음.

그러므로 최대한 쪼개서 작업을 하는게 좋음.

