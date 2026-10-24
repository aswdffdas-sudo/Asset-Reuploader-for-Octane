package console

import (
	"fmt"
	"os"
	"os/exec"
	"runtime"
)

func ClearScreen() {
	var cmd *exec.Cmd
	switch runtime.GOOS {
	case "windows":
		cmd = exec.Command("cmd", "/c", "cls")
	default:
		cmd = exec.Command("clear")
	}
	cmd.Stdout = os.Stdout
	if err := cmd.Run(); err != nil && runtime.GOOS != "windows" {
		// minimal installs may lack ncurses' clear; fall back to ANSI escapes
		fmt.Print("\x1b[H\x1b[2J")
	}
}
