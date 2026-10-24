package files

import (
	"os"
	"path/filepath"
)

// baseDir returns the folder that config.ini and the cookie files live in.
// The current directory wins when it has a config.ini (go run, terminal use);
// otherwise the executable's folder is used, since Linux file managers often
// start programs with the home directory as the working directory.
func baseDir() (string, error) {
	wd, err := os.Getwd()
	if err != nil {
		return "", err
	}
	if _, err := os.Stat(filepath.Join(wd, "config.ini")); err == nil {
		return wd, nil
	}

	exe, err := os.Executable()
	if err != nil {
		return wd, nil
	}
	if resolved, err := filepath.EvalSymlinks(exe); err == nil {
		exe = resolved
	}
	return filepath.Dir(exe), nil
}

func Write(n, c string) error {
	dir, err := baseDir()
	if err != nil {
		return err
	}

	// 0600: these files hold session cookies and API keys
	f, err := os.OpenFile(filepath.Join(dir, n), os.O_WRONLY|os.O_CREATE|os.O_TRUNC, 0o600)
	if err != nil {
		return err
	}
	defer f.Close()

	_, err = f.WriteString(c)
	return err
}

func Read(n string) (string, error) {
	dir, err := baseDir()
	if err != nil {
		return "", err
	}

	data, err := os.ReadFile(filepath.Join(dir, n))
	return string(data), err
}
