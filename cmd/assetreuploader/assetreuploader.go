package main

import (
	"errors"
	"fmt"
	"io"
	"log"
	"os"
	"strconv"
	"strings"

	"github.com/kartFr/Asset-Reuploader/internal/app/config"
	"github.com/kartFr/Asset-Reuploader/internal/color"
	"github.com/kartFr/Asset-Reuploader/internal/console"
	"github.com/kartFr/Asset-Reuploader/internal/files"
	"github.com/kartFr/Asset-Reuploader/internal/roblox"
)

var (
	cookieFile = config.Get("cookie_file")
	port       = config.Get("port")
)

func main() {
	console.ClearScreen()

	ensureUserID()

	fmt.Println("Authenticating cookie...")

	cookie, readErr := files.Read(cookieFile)

	// Parse cookie file (Roblox cookie + optional legacy api-key: line)
	roblosCookie, legacyAPIKey := parseCookieFile(cookie)
	roblosCookie = strings.TrimSpace(roblosCookie)
	legacyAPIKey = strings.TrimSpace(legacyAPIKey)
	if legacyAPIKey != "" && strings.TrimSpace(config.Get("api_key")) == "" {
		config.Set("api_key", legacyAPIKey)
		if err := config.PersistAPIKey(); err != nil {
			color.Error.Println("Failed to save API key to ", config.Get("api_key_file"), ": ", err)
		}
	}

	c, clientErr := roblox.NewClient(roblosCookie)
	console.ClearScreen()

	if readErr != nil || clientErr != nil {
		if readErr != nil && !os.IsNotExist(readErr) {
			color.Error.Println(readErr)
		}

		if clientErr != nil && roblosCookie != "" {
			color.Error.Println(clientErr)
		}

		getCookie(c)
	}

	if err := saveCookieFile(c.Cookie); err != nil {
		color.Error.Println("Failed to save cookie: ", err)
	}

	// Load optional official Roblox cookie for downloading original assets
	robloxCookieFile := config.Get("roblox_cookie_file")
	if robloxCookieFile == "" {
		robloxCookieFile = "roblox_cookie.txt"
	}
	rData, rErr := files.Read(robloxCookieFile)
	if rErr == nil && strings.TrimSpace(rData) != "" {
		rCookie, _ := parseCookieFile(rData)
		c.RobloxCookie = strings.TrimSpace(rCookie)
		fmt.Println("Official Roblox download cookie loaded!")
	} else {
		fmt.Println("Tip: Put your official Roblox .ROBLOSECURITY in roblox_cookie.txt to download private assets.")
	}
	
	ensureAPIKey()

	fmt.Println("localhost started on port " + port + ". Waiting to start reuploading.")
	if err := serve(c); err != nil {
		log.Fatal(err)
	}
}

// parseCookieFile extracts the Roblox cookie and API key from the cookie file
func parseCookieFile(content string) (roblosCookie, apiKey string) {
	lines := strings.Split(strings.TrimSpace(content), "\n")
	for _, line := range lines {
		line = strings.TrimSpace(line)
		if line == "" || strings.HasPrefix(line, "#") || strings.HasPrefix(line, "//") {
			continue
		}
		if strings.HasPrefix(line, "api-key:") {
			apiKey = strings.TrimPrefix(line, "api-key:")
			apiKey = strings.TrimSpace(apiKey)
		} else {
			roblosCookie = line
		}
	}
	return roblosCookie, apiKey
}

// saveCookieFile saves the Roblox cookie to cookie.txt (API key lives in api-key.txt).
func saveCookieFile(roblosCookie string) error {
	return files.Write(cookieFile, strings.TrimSpace(roblosCookie)+"\n")
}

func getCookie(c *roblox.Client) {
	for {
		i, err := console.LongInput("ROBLOSECURITY: ")
		console.ClearScreen()
		if errors.Is(err, io.EOF) {
			// no terminal attached (e.g. started from a file manager); retrying would spin forever
			log.Fatal("No cookie in ", cookieFile, " and no terminal to ask for one. Run ./start.sh from a terminal.")
		}
		if err != nil {
			color.Error.Println(err)
			continue
		}

		fmt.Println("Authenticating cookie...")
		err = c.SetCookie(i)
		console.ClearScreen()
		if err != nil {
			color.Error.Println(err)
			continue
		}

		break
	}
}

// ensureUserID asks for the user's own revival user ID when config.ini has none,
// since revivals like Octane fall back to it when cookie authentication fails.
func ensureUserID() {
	domain := strings.TrimSpace(config.Get("domain"))
	if domain == "" || strings.Contains(domain, "roblox.com") {
		return
	}
	if strings.TrimSpace(config.Get("user_id")) != "" {
		return
	}

	fmt.Printf("Enter your %s user ID (the number in your profile URL, e.g. https://%s/users/12345/profile).\n", domain, domain)
	for {
		id, err := console.Input("User ID: ")
		if errors.Is(err, io.EOF) {
			log.Fatal("user_id is empty in config.ini and there is no terminal to ask for it. Set it in config.ini or run ./start.sh from a terminal.")
		}
		if err != nil {
			color.Error.Println(err)
			continue
		}

		if _, err := strconv.ParseUint(id, 10, 64); err != nil {
			color.Error.Println("User ID must be a number.")
			continue
		}

		config.Set("user_id", id)
		if err := config.Save(); err != nil {
			color.Error.Println("Failed to save config: ", err)
		}
		break
	}
	console.ClearScreen()
}

func ensureAPIKey() {
	domain := strings.TrimSpace(config.Get("domain"))
	if domain != "" && !strings.Contains(domain, "roblox.com") {
		// Revivals (Aisaka, Madxka, etc.) do not use Roblox Open Cloud API keys
		return
	}

	currentAPIKey := strings.TrimSpace(config.Get("api_key"))
	if currentAPIKey != "" {
		return
	}

	fmt.Println("Enter your Open Cloud API key to enable mesh/animation uploads.")
	fmt.Println("How to get one:")
	fmt.Println("1. Go to https://create.roblox.com/dashboard/credentials?activeTab=ApiKeysTab")
	fmt.Println("2. Click Create API Key")
	fmt.Println("3. Enter any name")
	fmt.Println("4. Select Assets in Select API System")
	fmt.Println("5. Select Write in each Assets permission")
	for {
		key, err := console.Input("API key: ")
		if err != nil {
			color.Error.Println(err)
			continue
		}

		key = strings.TrimSpace(key)
		if key == "" {
			color.Error.Println("API key is required.")
			continue
		}

		config.Set("api_key", key)
		if err := config.PersistAPIKey(); err != nil {
			color.Error.Println("Failed to save api key: ", err)
			continue
		}
		if err := config.Save(); err != nil {
			color.Error.Println("Failed to save config: ", err)
			continue
		}
		break
	}
}
