document.addEventListener("DOMContentLoaded", function () {

    const chatbot = document.getElementById("gymChatbot");
    const toggleButton = document.getElementById("gymChatbotToggle");
    const closeButton = document.getElementById("gymChatbotClose");

    const windowElement =
        document.getElementById("gymChatbotWindow");

    const messagesContainer =
        document.getElementById("gymChatbotMessages");

    const input =
        document.getElementById("gymChatbotInput");

    const sendButton =
        document.getElementById("gymChatbotSend");

    if (!chatbot ||
        !toggleButton ||
        !closeButton ||
        !windowElement ||
        !messagesContainer ||
        !input ||
        !sendButton) {
        return;
    }

    // =========================================================
    // OPEN / CLOSE
    // =========================================================

    function openChat() {

        chatbot.classList.add("is-open");

        chatbot.setAttribute(
            "aria-hidden",
            "false");

        toggleButton.setAttribute(
            "aria-expanded",
            "true");

        setTimeout(function () {
            input.focus();
        }, 150);
    }

    function closeChat() {

        chatbot.classList.remove("is-open");

        chatbot.setAttribute(
            "aria-hidden",
            "true");

        toggleButton.setAttribute(
            "aria-expanded",
            "false");
    }

    toggleButton.addEventListener(
        "click",
        openChat);

    closeButton.addEventListener(
        "click",
        closeChat);

    // =========================================================
    // SAFE MARKDOWN FORMATTER
    // =========================================================

    function escapeHtml(text) {

        const div =
            document.createElement("div");

        div.textContent = text;

        return div.innerHTML;
    }

    function formatBotMessage(text) {

        let safe =
            escapeHtml(text);

        // Bold: **text**
        safe = safe.replace(
            /\*\*(.+?)\*\*/g,
            "<strong>$1</strong>"
        );

        // Convert bullet lines
        safe = safe.replace(
            /^\s*[\*\-]\s+(.+)$/gm,
            "<span class=\"chatbot-bullet\">• $1</span>"
        );

        // Preserve line breaks
        safe = safe.replace(
            /\r?\n/g,
            "<br>"
        );

        return safe;
    }

    // =========================================================
    // ADD MESSAGE
    // =========================================================

    function addMessage(
        text,
        sender = "bot") {

        const message =
            document.createElement("div");

        message.className =
            sender === "user"
                ? "gym-chatbot-message gym-chatbot-message-user"
                : "gym-chatbot-message gym-chatbot-message-bot";

        if (sender === "user") {

            message.innerHTML = `
                <div class="gym-chatbot-message-content">
                    <div class="gym-chatbot-message-bubble"></div>
                </div>
            `;

            message
                .querySelector(
                    ".gym-chatbot-message-bubble")
                .textContent = text;
        }
        else {

            message.innerHTML = `
                <div class="gym-chatbot-message-avatar">
                    <i class="fa fa-bolt"></i>
                </div>

                <div class="gym-chatbot-message-content">
                    <div class="gym-chatbot-message-bubble"></div>
                </div>
            `;

            message
                .querySelector(
                    ".gym-chatbot-message-bubble")
                .innerHTML =
                formatBotMessage(text);
        }

        messagesContainer.appendChild(message);

        scrollToBottom();
    }

    // =========================================================
    // TYPING
    // =========================================================

    function showTyping() {

        removeTyping();

        const typing =
            document.createElement("div");

        typing.id =
            "gymChatbotTyping";

        typing.className =
            "gym-chatbot-message gym-chatbot-message-bot";

        typing.innerHTML = `
            <div class="gym-chatbot-message-avatar">
                <i class="fa fa-bolt"></i>
            </div>

            <div class="gym-chatbot-message-content">
                <div class="gym-chatbot-message-bubble gym-chatbot-typing">
                    <span></span>
                    <span></span>
                    <span></span>
                </div>
            </div>
        `;

        messagesContainer.appendChild(typing);

        scrollToBottom();
    }

    function removeTyping() {

        const typing =
            document.getElementById(
                "gymChatbotTyping");

        if (typing) {
            typing.remove();
        }
    }

    // =========================================================
    // SCROLL
    // =========================================================

    function scrollToBottom() {

        messagesContainer.scrollTop =
            messagesContainer.scrollHeight;
    }

    // =========================================================
    // SEND MESSAGE
    // =========================================================

    let isSending = false;

    async function sendMessage() {

        if (isSending) {
            return;
        }

        const message =
            input.value.trim();

        if (!message) {
            input.focus();
            return;
        }

        if (message.length > 1000) {

            addMessage(
                "الرسالة طويلة جدًا. الحد الأقصى 1000 حرف.",
                "bot");

            return;
        }

        isSending = true;

        input.value = "";

        input.disabled = true;

        sendButton.disabled = true;

        addMessage(
            message,
            "user");

        showTyping();

        try {

            const tokenElement =
                document.querySelector(
                    'input[name="__RequestVerificationToken"]');

            const token =
                tokenElement
                    ? tokenElement.value
                    : "";

            const response =
                await fetch(
                    "/Chatbot/SendMessage",
                    {
                        method: "POST",

                        headers: {
                            "Content-Type":
                                "application/json",

                            "RequestVerificationToken":
                                token
                        },

                        body: JSON.stringify({
                            message: message
                        })
                    });

            const data =
                await response.json();

            removeTyping();

            if (response.ok &&
                data.success &&
                data.reply) {

                addMessage(
                    data.reply,
                    "bot");

            }
            else {

                addMessage(
                    data.message ||
                    "حدث خطأ أثناء معالجة الرسالة. حاول مرة أخرى.",
                    "bot");
            }

        }
        catch (error) {

            console.error(
                "GYM AI Error:",
                error);

            removeTyping();

            addMessage(
                "حصلت مشكلة في الاتصال بالخدمة. حاول مرة أخرى.",
                "bot");
        }
        finally {

            isSending = false;

            input.disabled = false;

            sendButton.disabled = false;

            input.focus();
        }
    }

    sendButton.addEventListener(
        "click",
        sendMessage);

    // =========================================================
    // ENTER TO SEND
    // SHIFT + ENTER = NEW LINE
    // =========================================================

    input.addEventListener(
        "keydown",
        function (event) {

            if (
                event.key === "Enter" &&
                !event.shiftKey
            ) {
                event.preventDefault();

                sendMessage();
            }
        });

    // =========================================================
    // AUTO RESIZE TEXTAREA
    // =========================================================

    input.addEventListener(
        "input",
        function () {

            input.style.height =
                "auto";

            input.style.height =
                Math.min(
                    input.scrollHeight,
                    120
                ) + "px";
        });
});