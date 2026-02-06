#!/bin/bash
# 一键启动开发环境（前端 + 后端）
# 自动检测并释放占用端口

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 端口配置
BACKEND_PORT=5241
FRONTEND_PORT=3000

# 项目路径（相对于脚本位置）
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
BACKEND_DIR="$PROJECT_ROOT/src/apps/api/ScaffoldGenerator.Api"
FRONTEND_DIR="$PROJECT_ROOT/src/apps/web-configurator"

# 日志函数
log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[OK]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# 检查端口是否被占用，返回占用进程的 PID
get_port_pid() {
    local port=$1
    lsof -ti ":$port" 2>/dev/null || true
}

# 释放端口
kill_port() {
    local port=$1
    local pids
    pids=$(get_port_pid "$port")

    if [[ -n "$pids" ]]; then
        log_warn "端口 $port 被占用，正在释放..."
        for pid in $pids; do
            local proc_name
            proc_name=$(ps -p "$pid" -o comm= 2>/dev/null || echo "unknown")
            log_info "  终止进程: PID=$pid ($proc_name)"
            kill -9 "$pid" 2>/dev/null || true
        done
        sleep 1

        # 验证端口已释放
        if [[ -n "$(get_port_pid "$port")" ]]; then
            log_error "无法释放端口 $port"
            return 1
        fi
        log_success "端口 $port 已释放"
    else
        log_success "端口 $port 可用"
    fi
}

# 检查依赖
check_dependencies() {
    log_info "检查依赖..."

    if ! command -v dotnet &> /dev/null; then
        log_error "未安装 dotnet CLI"
        exit 1
    fi

    if ! command -v pnpm &> /dev/null; then
        log_error "未安装 pnpm"
        exit 1
    fi

    log_success "依赖检查通过"
}

# 启动后端
start_backend() {
    log_info "启动后端服务 (端口: $BACKEND_PORT)..."

    if [[ ! -d "$BACKEND_DIR" ]]; then
        log_error "后端目录不存在: $BACKEND_DIR"
        exit 1
    fi

    cd "$BACKEND_DIR"
    dotnet run --urls "http://localhost:$BACKEND_PORT" &
    BACKEND_PID=$!

    # 等待后端启动
    local max_wait=30
    local waited=0
    while [[ -z "$(get_port_pid $BACKEND_PORT)" ]] && [[ $waited -lt $max_wait ]]; do
        sleep 1
        ((waited++))
        echo -n "."
    done
    echo ""

    if [[ -n "$(get_port_pid $BACKEND_PORT)" ]]; then
        log_success "后端已启动: http://localhost:$BACKEND_PORT"
    else
        log_error "后端启动超时"
        exit 1
    fi
}

# 启动前端
start_frontend() {
    log_info "启动前端服务 (端口: $FRONTEND_PORT)..."

    if [[ ! -d "$FRONTEND_DIR" ]]; then
        log_error "前端目录不存在: $FRONTEND_DIR"
        exit 1
    fi

    cd "$FRONTEND_DIR"

    # 检查 node_modules
    if [[ ! -d "node_modules" ]]; then
        log_warn "node_modules 不存在，正在安装依赖..."
        pnpm install
    fi

    # 启动 Vite，覆盖代理目标为实际后端端口
    VITE_API_TARGET="http://localhost:$BACKEND_PORT" pnpm dev --port $FRONTEND_PORT &
    FRONTEND_PID=$!

    # 等待前端启动
    local max_wait=30
    local waited=0
    while [[ -z "$(get_port_pid $FRONTEND_PORT)" ]] && [[ $waited -lt $max_wait ]]; do
        sleep 1
        ((waited++))
        echo -n "."
    done
    echo ""

    if [[ -n "$(get_port_pid $FRONTEND_PORT)" ]]; then
        log_success "前端已启动: http://localhost:$FRONTEND_PORT"
    else
        log_error "前端启动超时"
        exit 1
    fi
}

# 清理函数
cleanup() {
    echo ""
    log_info "正在停止服务..."

    if [[ -n "$BACKEND_PID" ]]; then
        kill "$BACKEND_PID" 2>/dev/null || true
    fi
    if [[ -n "$FRONTEND_PID" ]]; then
        kill "$FRONTEND_PID" 2>/dev/null || true
    fi

    # 确保端口释放
    kill_port $BACKEND_PORT 2>/dev/null || true
    kill_port $FRONTEND_PORT 2>/dev/null || true

    log_success "服务已停止"
    exit 0
}

# 注册清理函数
trap cleanup SIGINT SIGTERM

# 主流程
main() {
    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}  NetTemplete 开发环境启动脚本${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""

    check_dependencies

    echo ""
    log_info "释放端口..."
    kill_port $BACKEND_PORT
    kill_port $FRONTEND_PORT

    echo ""
    start_backend
    start_frontend

    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}  开发环境已就绪${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    echo -e "  前端: ${BLUE}http://localhost:$FRONTEND_PORT${NC}"
    echo -e "  后端: ${BLUE}http://localhost:$BACKEND_PORT${NC}"
    echo ""
    echo -e "  按 ${YELLOW}Ctrl+C${NC} 停止所有服务"
    echo ""

    # 保持脚本运行
    wait
}

main "$@"
